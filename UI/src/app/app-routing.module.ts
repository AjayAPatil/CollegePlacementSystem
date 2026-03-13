import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

const routes: Routes = [
  // {
  //   path: 'default',
  //   loadChildren: () => import('./features/default/default.module').then((m) => m.DefaultModule),
  // },
  {
    path: 'student',
    loadChildren: () => import('./modules').then((m) => m.StudentModule),
  },
  {
    path: 'company',
    loadChildren: () => import('./modules').then((m) => m.CompanyModule),
  },
  {
    path: 'admin',
    loadChildren: () => import('./modules').then((m) => m.AdminModule),
  },
  {
    path: 'auth',
    loadChildren: () =>
      import('./modules').then(m => m.AuthModule)
  },
  {
    path: 'login',
    loadChildren: () =>
      import('./modules').then(m => m.AuthModule)
  },
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full',
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule { }
