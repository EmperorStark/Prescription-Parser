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
  parsedSig$: Observable<SigResponse>;

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
      console.log('done');
    }
    

}
