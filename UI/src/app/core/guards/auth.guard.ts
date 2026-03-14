import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router } from '@angular/router';
import { GlobalService } from '../services/global.service';

@Injectable({
    providedIn: 'root'
})
export class AuthGuard implements CanActivate {

    constructor(
        private router: Router,
        private globalService: GlobalService
    ) { }

    canActivate(route: ActivatedRouteSnapshot): boolean {

        const role = localStorage.getItem('role');

        if (!role) {
            this.router.navigate(['/login']);
            return false;
        }

        const allowedRoles = route.data['roles'];

        if (allowedRoles && allowedRoles.includes(role)) {
            return true;
        }

        this.router.navigate(['/unauthorized']);
        return false;
    }
}