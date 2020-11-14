import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { take } from 'rxjs/operators';
import { ParseSigService } from 'src/app/services/parse-sig.service';
import { SigResponse } from '../Models/SigResponse';
import { FormBuilder } from '@angular/forms';
import { Validators } from '@angular/forms';
import { DrugTime } from '../Models/drugTime';

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
  drugTimes: DrugTime[] = [];
  alert = 0;

  maxYear = new Date().getFullYear() + 10; // oldest recorded person was 122 so only let calendar go back 125 years
  maxDate = new Date(this.maxYear, 0, 1); // create date based on the min year
  minDate = new Date(); // today

  displayedColumns: string[] = ['DrugName', 'Dose', 'Route', 'Disorder', 'Caution', 'Time'];

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
      .subscribe((res) => {
        if (res.length > 0) {
          this.alert = 1;
          this.parsedSig = res;
          for (let j = 0; j < res.length; j++) {
            var interval = this.intervalToMilliseconds(new Date(Date.parse(res[j].time)));
            let n: number;
            n = <any>setTimeout(this.setAlert, interval, j, res);
          }
        } else {
          this.alert = 2;
        }
      });
  }

  setAlert = (j: any, drugs: any[]) => {
    console.log('in alert');
    if (drugs && j < drugs.length) {
      alert(drugs[j].drug.dose + ' of ' + drugs[j].drug.name + ' ' + drugs[j].drug.route + ' ' + drugs[j].drug.caution);
    }
  };

  padZeroes(minutes: number) {
    if (minutes < 10) {
      return '0' + minutes;
    }
    return minutes;
  }

  onGenerateSchedule() {
    this.drugTimes = [];
    this.sigService
      .getDrugTimes(this.currentDate.getFullYear(), this.currentDate.getMonth() + 1, this.currentDate.getDate())
      .subscribe((res) => {
        for (let i = 0; i < res.length; i++) {
          var date = new Date(Date.parse(res[i].time));
          this.drugTimes.push(new DrugTime(res[i], date))
        }
        console.log(res[0].drug.dose);
      });
  }

  intervalToMilliseconds(drugTime: Date) {
    var curDate = new Date();
    var difference = drugTime.getTime() - curDate.getTime() + 2000;
    console.log(difference);
    return difference;
  }

  generateSchedule() {
    console.log(this.currentDate.getMonth());
  }

  clearAlert() {
    this.alert = 0;
  }


}
