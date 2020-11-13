import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { take } from 'rxjs/operators';
import { ParseSigService } from 'src/app/services/parse-sig.service';
import { SigResponse } from '../Models/SigResponse';
import { FormBuilder } from '@angular/forms';
import { Validators } from '@angular/forms';

export interface PeriodicElement {
  name: string;
  position: number;
  weight: number;
  symbol: string;
}

const ELEMENT_DATA: PeriodicElement[] = [
  {position: 1, name: 'Hydrogen', weight: 1.0079, symbol: 'H'},
  {position: 2, name: 'Helium', weight: 4.0026, symbol: 'He'},
  {position: 3, name: 'Lithium', weight: 6.941, symbol: 'Li'},
  {position: 4, name: 'Beryllium', weight: 9.0122, symbol: 'Be'},
  {position: 5, name: 'Boron', weight: 10.811, symbol: 'B'},
  {position: 6, name: 'Carbon', weight: 12.0107, symbol: 'C'},
  {position: 7, name: 'Nitrogen', weight: 14.0067, symbol: 'N'},
  {position: 8, name: 'Oxygen', weight: 15.9994, symbol: 'O'},
  {position: 9, name: 'Fluorine', weight: 18.9984, symbol: 'F'},
  {position: 10, name: 'Neon', weight: 20.1797, symbol: 'Ne'},
];

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

  dataSource = ELEMENT_DATA;

  elements: any = [
    {id: 1, first: 'Mark', last: 'Otto', handle: '@mdo'},
    {id: 2, first: 'Jacob', last: 'Thornton', handle: '@fat'},
    {id: 3, first: 'Larry', last: 'the Bird', handle: '@twitter'},
  ];

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
    this.drugTimes = [];
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
