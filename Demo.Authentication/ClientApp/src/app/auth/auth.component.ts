import { Component, Inject, OnInit} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { UserInfo } from './user-info';
import {
  OidcClientNotification,
  OidcSecurityService,
  OpenIdConfiguration,
  UserDataResult
} from "angular-auth-oidc-client";
import { Observable } from 'rxjs';

@Component({
  selector: 'app-auth',
  templateUrl: './auth.component.html',
  styleUrls: ['./auth.component.css']
})
export class AuthComponent implements OnInit {
  // @ts-ignore
  configuration$: Observable<OpenIdConfiguration>;
  // @ts-ignore
  userDataChanged$: Observable<OidcClientNotification<any>>;
  // @ts-ignore
  userData$: Observable<UserDataResult>;
  isAuthenticated = false;
  public loginField: string;
  public password: string;
  public baseUrl: string;
  public userInfo: UserInfo = {
    claims: [],
    isAuthenticated: false,
    scheme: ''
  };

  constructor(
    private readonly httpClient: HttpClient,
    @Inject('BASE_URL') baseUrl: string,
    public oidcSecurityService: OidcSecurityService) {
    this.loginField = "";
    this.password = "";
    this.baseUrl = baseUrl;
  }

  ngOnInit() {
    this.configuration$ = this.oidcSecurityService.getConfiguration();
    this.userData$ = this.oidcSecurityService.userData$;
    this.oidcSecurityService.isAuthenticated$.subscribe(
      ({ isAuthenticated }) => {
        this.isAuthenticated = isAuthenticated;
        console.warn('authenticated: ', isAuthenticated);
      }
    );
  }

  login() {
    this.oidcSecurityService.authorize();
  }

  refreshSession() {
    this.oidcSecurityService
      .forceRefreshSession()
      .subscribe((result) => console.log(result));
  }

  logout() {
    this.oidcSecurityService
      .logoff()
      .subscribe((result) => console.log(result));
  }

  logoffAndRevokeTokens() {
    this.oidcSecurityService
      .logoffAndRevokeTokens()
      .subscribe((result) => console.log(result));
  }

  revokeRefreshToken() {
    this.oidcSecurityService
      .revokeRefreshToken()
      .subscribe((result) => console.log(result));
  }

  revokeAccessToken() {
    this.oidcSecurityService
      .revokeAccessToken()
      .subscribe((result) => console.log(result));
  }

  public loadUserInfo(): void {
    this
      .httpClient
      .post<UserInfo>(`${this.baseUrl}/protected/getUserInfo`, null)
      .subscribe((data) =>  {
        this.userInfo = data
      });
  }

  public authenticate(login: string, password: string): void {
    this.login();
  }
  public checkJwtProtectedRoute(): void {
    this
      .httpClient
      .post(`${this.baseUrl}/protected/methodRequiringAuthorization`, null)
      .subscribe(result => {
          alert(result);
        },
          error => alert(error.status));
  }
  public checkAnonymousRoute(): void {
    this
      .httpClient
      .post(`${this.baseUrl}/protected/method`, null)
      .subscribe(result => alert(result), error => alert(error.status));
  }
}
