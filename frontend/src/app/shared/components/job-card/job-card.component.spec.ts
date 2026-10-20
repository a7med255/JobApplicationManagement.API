import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { SalaryPeriod } from '../../../core/models/job.models';
import { JobCardComponent } from './job-card.component';

describe('JobCardComponent', () => {
  let fixture: ComponentFixture<JobCardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [JobCardComponent],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(JobCardComponent);
  });

  it('renders visible salary data and emits the job id when applying', () => {
    fixture.componentRef.setInput('job', {
      id: 42,
      title: 'Frontend Engineer',
      isActive: true,
      minimumSalary: 20_000,
      maximumSalary: 30_000,
      currency: 'egp',
      salaryPeriod: SalaryPeriod.Monthly,
      isSalaryVisible: true,
    });
    fixture.componentRef.setInput('showApply', true);
    const apply = vi.fn();
    fixture.componentInstance.apply.subscribe(apply);
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).toContain('20,000–30,000 EGP / month');
    (fixture.nativeElement.querySelector('.job-card__actions button') as HTMLButtonElement).click();
    expect(apply).toHaveBeenCalledWith(42);
  });

  it('does not render salary values when the salary is hidden', () => {
    fixture.componentRef.setInput('job', {
      id: 7,
      title: 'Private Salary Role',
      isActive: true,
      minimumSalary: 99_999,
      isSalaryVisible: false,
    });
    fixture.detectChanges();

    expect(fixture.nativeElement.textContent).not.toContain('99,999');
  });
});
