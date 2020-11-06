import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { take } from 'rxjs/operators';
import { ParseSigService } from 'src/app/services/parse-sig.service';
import { SigResponse } from '../Models/SigResponse';

@Component({
  selector: 'app-sig-input',
  templateUrl: './sig-input.component.html',
  styleUrls: ['./sig-input.component.scss']
})
export class SigInputComponent implements OnInit {
  
  sigText : string;
  parsedSig$: Observable<SigResponse[]>;
  currentDate: Date;

  maxYear = new Date().getFullYear() + 10; // oldest recorded person was 122 so only let calendar go back 125 years
  maxDate = new Date(this.maxYear, 0, 1); // create date based on the min year
  minDate = new Date(); // today

  constructor(private sigService : ParseSigService) { }

  ngOnInit(): void {
  }

  onTextSubmitted(){
    // this.sigService.parseSig(this.sigText).subscribe(res => {
    //   console.log('IMHERE');
    //  console.log(JSON.stringify(res));
    //   console.log(res);
    // });
    this.parsedSig$ = this.sigService
      .parseSig(this.sigText)
      .pipe(take(1));
    }

  generateSchedule(){
    console.log(this.currentDate.getMonth());
  }
    

}
