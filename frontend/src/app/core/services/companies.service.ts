import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Company, UpdateCompanyRequest } from '../models/company.models';

@Injectable({ providedIn: 'root' })
export class CompaniesService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiBaseUrl}/companies/mine`;

  getMine(): Observable<Company> {
    return this.http.get<Company>(this.apiUrl);
  }

  updateMine(request: UpdateCompanyRequest): Observable<Company> {
    return this.http.put<Company>(this.apiUrl, request);
  }

  uploadLogo(file: File): Observable<Company> {
    const data = new FormData();
    data.append('file', file, file.name);
    return this.http.post<Company>(`${this.apiUrl}/logo`, data);
  }
}
