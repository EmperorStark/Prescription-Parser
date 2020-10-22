import { TestBed } from '@angular/core/testing';

import { ParseSigService } from './parse-sig.service';

describe('ParseSigService', () => {
  let service: ParseSigService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ParseSigService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
