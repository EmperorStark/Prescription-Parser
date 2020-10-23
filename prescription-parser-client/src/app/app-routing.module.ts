import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { SigInputComponent } from './Features/sig-input/sig-input.component';

const routes: Routes = [
  {
    path: 'sig-input',
    component: SigInputComponent,
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
  // bootstrap: [AppComponent],
})
export class AppRoutingModule { }
