using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.CandidateCvs.Commands;
using JobApplicationManagement.Application.Features.CandidateCvs.DTOs;
using JobApplicationManagement.Application.Features.CandidateCvs.Queries;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Domain.Enums;
using JobApplicationManagement.Tests.Features.SavedJobs;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using AppValidationException = JobApplicationManagement.Application.Common.Exceptions.ValidationException;

namespace JobApplicationManagement.Tests.Features.Candidates;

public class CandidateCvTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly Mock<IFileStorageService> _fileStorageMock = new();
    private readonly IOptions<FileUploadOptions> _options = Options.Create(new FileUploadOptions
    {
        MaximumCvBytes = 5 * 1024 * 1024
    });

    private static byte[] CreateValidPdfBytes() =>
        Encoding.UTF8.GetBytes("%PDF-1.5\n1 0 obj\n<< /Type /Catalog >>\nendobj\ntrailer\n<< /Root 1 0 R >>\n%%EOF");

    [Fact]
    public async Task Upload_Throws_ValidationException_When_File_Is_Not_Pdf()
    {
        var nonPdfContent = Encoding.UTF8.GetBytes("This is plain text, not a PDF");
        var handler = new UploadCandidateCvCommandHandler(
            _unitOfWorkMock.Object, _currentUserServiceMock.Object, _fileStorageMock.Object, _options);

        var command = new UploadCandidateCvCommand(
            "My Resume", "document.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", nonPdfContent, false);

        await Assert.ThrowsAsync<AppValidationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Upload_Throws_ValidationException_When_Pdf_Header_Missing()
    {
        var corruptedPdf = Encoding.UTF8.GetBytes("NO_MAGIC_HEADER_corrupted_file_content%%EOF");
        var handler = new UploadCandidateCvCommandHandler(
            _unitOfWorkMock.Object, _currentUserServiceMock.Object, _fileStorageMock.Object, _options);

        var command = new UploadCandidateCvCommand(
            "My Resume", "document.pdf", "application/pdf", corruptedPdf, false);

        await Assert.ThrowsAsync<AppValidationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Upload_Throws_ValidationException_When_File_Exceeds_5Mb()
    {
        var largeContent = new byte[6 * 1024 * 1024]; // 6MB
        var header = Encoding.UTF8.GetBytes("%PDF-");
        Array.Copy(header, largeContent, header.Length);

        var handler = new UploadCandidateCvCommandHandler(
            _unitOfWorkMock.Object, _currentUserServiceMock.Object, _fileStorageMock.Object, _options);

        var command = new UploadCandidateCvCommand(
            "My Resume", "large.pdf", "application/pdf", largeContent, false);

        await Assert.ThrowsAsync<AppValidationException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Upload_Succeeds_For_Valid_Pdf_And_Persists_Cloudinary_Key()
    {
        var candidate = new Candidate { Id = 1, UserId = "user-100", Name = "Candidate One" };
        _currentUserServiceMock.Setup(c => c.UserId).Returns("user-100");
        _unitOfWorkMock.Setup(u => u.Candidates.Query())
            .Returns(new SavedJobTestAsyncEnumerable<Candidate>(new[] { candidate }));
        _unitOfWorkMock.Setup(u => u.CandidateCvs.Query())
            .Returns(new SavedJobTestAsyncEnumerable<CandidateCv>(Array.Empty<CandidateCv>()));

        var validPdf = CreateValidPdfBytes();
        _fileStorageMock.Setup(f => f.UploadPrivatePdfAsync(validPdf, "resume.pdf", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoredFile("careerly/cvs/abc123.pdf", "Cloudinary", "raw", "authenticated", validPdf.Length));

        CandidateCv? savedCv = null;
        _unitOfWorkMock.Setup(u => u.CandidateCvs.AddAsync(It.IsAny<CandidateCv>()))
            .Callback<CandidateCv>(cv => savedCv = cv)
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new UploadCandidateCvCommandHandler(
            _unitOfWorkMock.Object, _currentUserServiceMock.Object, _fileStorageMock.Object, _options);

        var command = new UploadCandidateCvCommand("Lead CV", "resume.pdf", "application/pdf", validPdf, true);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Lead CV", result.DisplayName);
        Assert.True(result.IsDefault);
        Assert.NotNull(savedCv);
        Assert.Equal("careerly/cvs/abc123.pdf", savedCv.StorageKey);
        Assert.Equal("Cloudinary", savedCv.StorageProvider);
        Assert.NotNull(savedCv.ChecksumSha256);
    }

    [Fact]
    public async Task Rename_Updates_DisplayName_When_Authorized()
    {
        var candidate = new Candidate { Id = 1, UserId = "user-100" };
        var cv = new CandidateCv { Id = 5, CandidateId = 1, DisplayName = "Old Name", OriginalFileName = "test.pdf" };

        _currentUserServiceMock.Setup(c => c.UserId).Returns("user-100");
        _unitOfWorkMock.Setup(u => u.Candidates.Query())
            .Returns(new SavedJobTestAsyncEnumerable<Candidate>(new[] { candidate }));
        _unitOfWorkMock.Setup(u => u.CandidateCvs.Query())
            .Returns(new SavedJobTestAsyncEnumerable<CandidateCv>(new[] { cv }));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new RenameCandidateCvCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
        var result = await handler.Handle(new RenameCandidateCvCommand(5, "New Fancy Name"), CancellationToken.None);

        Assert.Equal("New Fancy Name", result.DisplayName);
        Assert.Equal("New Fancy Name", cv.DisplayName);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Rename_Throws_ValidationException_When_DisplayName_Is_Empty()
    {
        var handler = new RenameCandidateCvCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
        await Assert.ThrowsAsync<AppValidationException>(() =>
            handler.Handle(new RenameCandidateCvCommand(5, "   "), CancellationToken.None));
    }

    [Fact]
    public async Task Delete_Soft_Deletes_And_Deletes_Cloudinary_When_Unreferenced()
    {
        var candidate = new Candidate { Id = 1, UserId = "user-100" };
        var cv = new CandidateCv
        {
            Id = 10,
            CandidateId = 1,
            DisplayName = "To Delete",
            IsDefault = false,
            StorageKey = "careerly/cvs/to_del.pdf",
            ResourceType = "raw",
            DeliveryType = "authenticated"
        };

        _currentUserServiceMock.Setup(c => c.UserId).Returns("user-100");
        _unitOfWorkMock.Setup(u => u.Candidates.Query())
            .Returns(new SavedJobTestAsyncEnumerable<Candidate>(new[] { candidate }));
        _unitOfWorkMock.Setup(u => u.CandidateCvs.Query())
            .Returns(new SavedJobTestAsyncEnumerable<CandidateCv>(new[] { cv }));
        _unitOfWorkMock.Setup(u => u.JobApplications.Query())
            .Returns(new SavedJobTestAsyncEnumerable<JobApplication>(Array.Empty<JobApplication>()));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _fileStorageMock.Setup(f => f.DeleteAsync("careerly/cvs/to_del.pdf", "raw", "authenticated", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new DeleteCandidateCvCommandHandler(
            _unitOfWorkMock.Object, _currentUserServiceMock.Object, _fileStorageMock.Object);

        await handler.Handle(new DeleteCandidateCvCommand(10), CancellationToken.None);

        Assert.True(cv.IsDeleted);
        Assert.NotNull(cv.DeletedAt);
        _fileStorageMock.Verify(f => f.DeleteAsync("careerly/cvs/to_del.pdf", "raw", "authenticated", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_Preserves_Cloudinary_When_Referenced_By_Application()
    {
        var candidate = new Candidate { Id = 1, UserId = "user-100" };
        var cv = new CandidateCv
        {
            Id = 10,
            CandidateId = 1,
            DisplayName = "Referenced CV",
            StorageKey = "careerly/cvs/referenced.pdf",
            ResourceType = "raw",
            DeliveryType = "authenticated"
        };
        var app = new JobApplication { Id = 99, SubmittedCvId = 10, CandidateId = 1, JobId = 2 };

        _currentUserServiceMock.Setup(c => c.UserId).Returns("user-100");
        _unitOfWorkMock.Setup(u => u.Candidates.Query())
            .Returns(new SavedJobTestAsyncEnumerable<Candidate>(new[] { candidate }));
        _unitOfWorkMock.Setup(u => u.CandidateCvs.Query())
            .Returns(new SavedJobTestAsyncEnumerable<CandidateCv>(new[] { cv }));
        _unitOfWorkMock.Setup(u => u.JobApplications.Query())
            .Returns(new SavedJobTestAsyncEnumerable<JobApplication>(new[] { app }));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new DeleteCandidateCvCommandHandler(
            _unitOfWorkMock.Object, _currentUserServiceMock.Object, _fileStorageMock.Object);

        await handler.Handle(new DeleteCandidateCvCommand(10), CancellationToken.None);

        Assert.True(cv.IsDeleted);
        // Must NOT delete from Cloudinary when an existing job application references it!
        _fileStorageMock.Verify(f => f.DeleteAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Download_Retrieves_Bytes_From_FileStorage()
    {
        var candidate = new Candidate { Id = 1, UserId = "user-100" };
        var cv = new CandidateCv
        {
            Id = 8,
            CandidateId = 1,
            DisplayName = "My CV",
            OriginalFileName = "resume.pdf",
            ContentType = "application/pdf",
            StorageKey = "careerly/cvs/downloadable.pdf",
            ResourceType = "raw",
            DeliveryType = "authenticated",
            Content = null
        };
        var expectedBytes = CreateValidPdfBytes();

        _currentUserServiceMock.Setup(c => c.UserId).Returns("user-100");
        _unitOfWorkMock.Setup(u => u.Candidates.Query())
            .Returns(new SavedJobTestAsyncEnumerable<Candidate>(new[] { candidate }));
        _unitOfWorkMock.Setup(u => u.CandidateCvs.Query())
            .Returns(new SavedJobTestAsyncEnumerable<CandidateCv>(new[] { cv }));
        _fileStorageMock.Setup(f => f.DownloadPrivateAsync("careerly/cvs/downloadable.pdf", "raw", "authenticated", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedBytes);

        var handler = new DownloadMyCandidateCvQueryHandler(
            _unitOfWorkMock.Object, _currentUserServiceMock.Object, _fileStorageMock.Object);

        var result = await handler.Handle(new DownloadMyCandidateCvQuery(8), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(expectedBytes, result.Content);
        Assert.Equal("application/pdf", result.ContentType);
        Assert.Equal("resume.pdf", result.FileName);
    }
}
