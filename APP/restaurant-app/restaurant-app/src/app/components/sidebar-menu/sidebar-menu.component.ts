import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';


@Component({
  selector: 'app-sidebar-menu',
  imports: [],
  templateUrl: './sidebar-menu.component.html',
  styleUrl: './sidebar-menu.component.scss'
})

export class SidebarMenuComponent {
  @Input() role: string = '';


  constructor (private router: Router) {}

  navigateTo (path: string) {
    this.router.navigate([path])
  }
}
