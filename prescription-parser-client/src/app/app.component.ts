import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { ParseSigService } from './services/parse-sig.service';
import { take } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'prescription-parser-client';
  sigText : string;
  parsedSig$: Observable<any>;

  constructor(private sigService : ParseSigService) { }

  onTextSubmitted(){
    this.parsedSig$ = this.sigService
      .parseSig(this.sigText)
      .pipe(take(1));
    }
  
}
