import { Component } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';

@Component({
  selector: 'app-student-profile',
  standalone: false,
  templateUrl: './student-profile.component.html',
  styleUrl: './student-profile.component.scss',
})
export class StudentProfileComponent {
  profileForm!: FormGroup;
  skills: string[] = ['Angular', '.NET', 'SQL'];

  constructor(private fb: FormBuilder) { }

  ngOnInit() {
    this.profileForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', Validators.required],
      department: [''],
      cgpa: ['']
    });
  }

  addSkill(skillInput: HTMLInputElement) {
    if (skillInput.value.trim()) {
      this.skills.push(skillInput.value.trim());
      skillInput.value = '';
    }
  }

  removeSkill(skill: string) {
    this.skills = this.skills.filter(s => s !== skill);
  }

  saveProfile() {
    if (this.profileForm.valid) {
      console.log(this.profileForm.value);
    }
  }
}
