import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ParseSigService {

  constructor(private http: HttpClient) { }

  public parseSig(sigText : string): Observable<string>{
    const params = new HttpParams();
    params.append('sigText', sigText);
    return this.http.get<string>('https://localhost:5001/api/parse');
    // return this.http.get<string>(`${environment.ApiBaseUrl}/parse`, {
    //   params
    // });
  }
}
