import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { take } from 'rxjs/operators';
import { ParseSigService } from 'src/app/services/parse-sig.service';
import { SigResponse } from '../Models/SigResponse';
import { FormBuilder } from '@angular/forms';
import { Validators } from '@angular/forms';

@Component({
  selector: 'app-sig-input',
  templateUrl: './sig-input.component.html',
  styleUrls: ['./sig-input.component.scss']
})
export class SigInputComponent implements OnInit {

  sigText: string;
  drugName: string;
  parsedSig: SigResponse[];
  currentDate: Date;
  drugTimes: any[] = [];
  alert = 0;

  maxYear = new Date().getFullYear() + 10; // oldest recorded person was 122 so only let calendar go back 125 years
  maxDate = new Date(this.maxYear, 0, 1); // create date based on the min year
  minDate = new Date(); // today

  displayedColumns: string[] = ['drugName', 'dose', 'route', 'disorder', 'caution', 'time'];

  sigForm = this.fb.group({
    drugName: ['', Validators.required],
    sigText: ['', Validators.required],
  });

  constructor(private sigService: ParseSigService, private fb: FormBuilder,) {
  }

  ngOnInit(): void {
  }

  onTextSubmitted() {
    console.log(this.sigText + '   ' + this.drugName);
    this.sigService
      .parseSig(this.sigText, this.drugName)
      .pipe(take(1))
      .subscribe((res) => {
        if (res.length > 0) {
          this.alert = 1;
          this.parsedSig = res;
        } else {
          this.alert = 2;
        }
      });
  }

  onGenerateSchedule() {
    this.sigService
      .getDrugTime(this.currentDate.getFullYear(), this.currentDate.getMonth() + 1, this.currentDate.getDate())
      .subscribe((res) => {
        this.drugTimes.push(res);
        console.log(this.drugTimes[0].drug.dose);
      });
  }

  generateSchedule() {
    console.log(this.currentDate.getMonth());
  }

  clearAlert() {
    this.alert = 0;
  }


}
