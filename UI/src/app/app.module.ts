import { NgModule, provideBrowserGlobalErrorListeners } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { HeaderComponent } from './layout';
import { AdminModule } from './modules/admin/admin.module';
import { AuthModule } from './modules/auth/auth.module';
import { CompanyModule } from './modules/company/company.module';
import { StudentModule } from './modules/student/student.module';
import { DATE_FORMATS, MY_YEAR_FORMATS, SharedModule } from './shared';
import { UnauthorizedComponent } from './shared/components/unauthorized/unauthorized.component';
import { MAT_DATE_FORMATS } from '@angular/material/core';
import { MatMomentDateModule } from '@angular/material-moment-adapter';

@NgModule({
  declarations: [
    AppComponent,
    HeaderComponent,
    UnauthorizedComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    SharedModule,
    StudentModule,
    AdminModule,
    CompanyModule,
    AuthModule,
    MatMomentDateModule
  ],
  providers: [
    provideBrowserGlobalErrorListeners(),
    //{ provide: MAT_DATE_FORMATS, useValue: DATE_FORMATS },
  ],
  bootstrap: [AppComponent],
})
export class AppModule { }
