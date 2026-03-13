import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AdminRoutingModule } from './admin-routing.module';
import { AdminComponent } from './admin.component';
import { AdminDashboardComponent } from './admin-dashboard/admin-dashboard.component';
import { SidenavComponent } from '../../layout';
import { MaterialModule } from '../../shared';

@NgModule({
  declarations: [
    AdminComponent,
    AdminDashboardComponent,
    SidenavComponent
  ],
  imports: [
    CommonModule,
    AdminRoutingModule,
    MaterialModule
  ],
})
export class AdminModule { }
