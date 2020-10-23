import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ParseSigService {

  constructor(private http: HttpClient) { }

  public parseSig(sigText : string): Observable<any>{
    return this.http.get<any>(`${environment.ApiBaseUrl}/parse?sigText=` + sigText);
  }
}
