import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { SigResponse } from '../Features/Models/SigResponse';

@Injectable({
  providedIn: 'root'
})
export class ParseSigService {

  constructor(private http: HttpClient) { }

  public parseSig(sigText : string, drugName : string): Observable<any[]>{
    const params = new HttpParams();
    params.append('Content-Type', 'application/json');
    return this.http.get<any[]>(`${environment.ApiBaseUrl}/parse?sigText=` + sigText + "&drugName=" + drugName, {
      params
    });
  }

  public getDrugTimes(year:number, month:number, day:number): Observable<any[]>{
    console.log('year: ' + year + ' month: ' + month + ' day: ' + day);
    const params = new HttpParams();
    params.append('Content-Type', 'application/json');
    params.append('year', year.toString());
    params.append('month', month.toString());
    params.append('day', day.toString());
    return this.http.get<any[]>(`${environment.ApiBaseUrl}/parse/drugTime?year=` + year + "&month=" + month + "&day=" + day, {
      params
    });
  }
}
