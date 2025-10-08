import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, RouterOutlet, NavigationEnd } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { filter } from 'rxjs/operators';

import { AuthService, User } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterModule,  // Added this - needed for routerLink and routerLinkActive
    MatToolbarModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  title = 'nexusboard-ui';
  currentUser: User | null = null;
  showNavigation = false;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Subscribe to current user changes
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
      this.updateNavigationVisibility();
    });

    // Listen to route changes to update navigation visibility
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => {
        this.updateNavigationVisibility();
      });
  }

  updateNavigationVisibility(): void {
    // Hide navigation on login and register pages
    const currentRoute = this.router.url;
    const publicRoutes = ['/login', '/register'];
    this.showNavigation = !publicRoutes.includes(currentRoute) && this.currentUser !== null;
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}