import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CandidateCv } from '../models/candidate-cv.models';

@Injectable({ providedIn: 'root' })
export class CandidateCvsService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiBaseUrl}/candidate-cvs`;

  getMine(): Observable<CandidateCv[]> {
    return this.http.get<CandidateCv[]>(this.apiUrl);
  }

  upload(displayName: string, file: File, setAsDefault: boolean): Observable<CandidateCv> {
    const formData = new FormData();
    formData.append('displayName', displayName);
    formData.append('file', file, file.name);
    formData.append('setAsDefault', String(setAsDefault));
    return this.http.post<CandidateCv>(this.apiUrl, formData);
  }

  setDefault(id: number): Observable<CandidateCv> {
    return this.http.put<CandidateCv>(`${this.apiUrl}/${id}/default`, {});
  }

  rename(id: number, displayName: string): Observable<CandidateCv> {
    return this.http.put<CandidateCv>(`${this.apiUrl}/${id}/name`, { displayName });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  download(id: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${id}/download`, { responseType: 'blob' });
  }
}
