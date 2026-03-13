import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-register-type-dialog',
  standalone: true,
  imports: [
    MatDialogModule,
    MatButtonModule
  ],
  template: `
    <h2 mat-dialog-title>Select Registration Type</h2>

    <mat-dialog-content>
      <p>Please choose how you want to register:</p>
    </mat-dialog-content>

    <mat-dialog-actions align="center">
      <button mat-raised-button color="primary" (click)="select('student')">
        Student
      </button>

      <button mat-raised-button color="accent" (click)="select('company')">
        Company
      </button>
    </mat-dialog-actions>
  `
})
export class RegisterTypeDialog {

  constructor(private dialogRef: MatDialogRef<RegisterTypeDialog>) {}

  select(type: 'student' | 'company') {
    this.dialogRef.close(type);
  }
}