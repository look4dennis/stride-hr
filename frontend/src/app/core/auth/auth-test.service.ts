import { Injectable } from '@angular/core';
import { AuthService } from './auth.service';
import { NotificationService } from '../services/notification.service';

@Injectable({
  providedIn: 'root'
})
export class AuthTestService {
  constructor(
    private authService: AuthService,
    private notificationService: NotificationService
  ) {}

  async testAuthenticationFlow(): Promise<void> {
    console.log('🔐 Testing Authentication Flow...');
    
    try {
      // Test 1: Check initial state
      console.log('1. Initial authentication state:', this.authService.isAuthenticated);
      
      // Test 2: Test invalid login
      console.log('2. Testing invalid login...');
      try {
        await this.authService.login({
          email: 'invalid@test.com',
          password: 'wrongpassword'
        }).toPromise();
      } catch (error) {
        console.log('✅ Invalid login correctly rejected:', error);
      }
      
      // Test 3: Test valid superadmin login
      console.log('3. Testing superadmin login...');
      try {
        const response = await this.authService.login({
          email: 'superadmin@stridehr.com',
          password: 'adminsuper2025$'
        }).toPromise();
        
        if (response?.success) {
          console.log('✅ Superadmin login successful:', response.data?.user);
          console.log('✅ Token received:', !!response.data?.token);
          console.log('✅ Refresh token received:', !!response.data?.refreshToken);
          
          // Test 4: Check authenticated state
          console.log('4. Authentication state after login:', this.authService.isAuthenticated);
          console.log('   Current user:', this.authService.currentUser);
          
          // Test 5: Test token validation
          console.log('5. Testing token validation...');
          const validationResponse = await this.authService.validateToken().toPromise();
          console.log('✅ Token validation result:', validationResponse);
          
          // Test 6: Test getting current user from server
          console.log('6. Testing get current user from server...');
          const userResponse = await this.authService.getCurrentUserFromServer().toPromise();
          console.log('✅ Current user from server:', userResponse);
          
          // Test 7: Test token refresh
          console.log('7. Testing token refresh...');
          const refreshResponse = await this.authService.refreshToken().toPromise();
          console.log('✅ Token refresh result:', refreshResponse?.success);
          
          // Test 8: Test logout
          console.log('8. Testing logout...');
          this.authService.logout();
          console.log('✅ Logout completed. Authentication state:', this.authService.isAuthenticated);
          
        } else {
          console.log('❌ Superadmin login failed:', response);
        }
      } catch (error) {
        console.log('❌ Superadmin login error:', error);
      }
      
      console.log('🔐 Authentication flow test completed!');
      this.notificationService.showSuccess('Authentication test completed. Check console for results.', 'Test Complete');
      
    } catch (error) {
      console.error('❌ Authentication test failed:', error);
      this.notificationService.showError('Authentication test failed. Check console for details.', 'Test Failed');
    }
  }

  testRoleBasedAccess(): void {
    console.log('🛡️ Testing Role-Based Access Control...');
    
    const currentUser = this.authService.currentUser;
    if (!currentUser) {
      console.log('❌ No authenticated user for role testing');
      return;
    }
    
    console.log('Current user roles:', currentUser.roles);
    console.log('Current user permissions:', currentUser.permissions);
    
    // Test role checks
    const testRoles = ['SuperAdmin', 'Admin', 'HR', 'Manager', 'Employee'];
    testRoles.forEach(role => {
      const hasRole = this.authService.hasRole(role);
      console.log(`Has role '${role}':`, hasRole);
    });
    
    // Test permission checks
    const testPermissions = ['*', 'user.create', 'user.read', 'user.update', 'user.delete'];
    testPermissions.forEach(permission => {
      const hasPermission = this.authService.hasPermission(permission);
      console.log(`Has permission '${permission}':`, hasPermission);
    });
    
    console.log('🛡️ Role-based access control test completed!');
  }

  async testSessionManagement(): Promise<void> {
    console.log('📱 Testing Session Management...');
    
    if (!this.authService.isAuthenticated) {
      console.log('❌ User must be authenticated for session testing');
      return;
    }
    
    try {
      // Test getting active sessions
      console.log('1. Getting active sessions...');
      const sessions = await this.authService.getActiveSessions().toPromise();
      console.log('✅ Active sessions:', sessions);
      
      // Test token expiry check
      console.log('2. Checking token validity...');
      const isValid = this.authService.isTokenValid();
      console.log('✅ Token is valid:', isValid);
      
      const expiry = this.authService.getTokenExpiry();
      console.log('✅ Token expires at:', expiry);
      
      console.log('📱 Session management test completed!');
      
    } catch (error) {
      console.error('❌ Session management test failed:', error);
    }
  }
}