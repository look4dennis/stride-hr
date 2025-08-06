import { routes } from './app.routes';

// Simple test to verify routing configuration
console.log('Testing routing configuration...');

let passedTests = 0;
let totalTests = 0;

// Test 1: Check if routes array exists and has content
totalTests++;
if (routes && routes.length > 0) {
  console.log('✓ Routes array exists and has content');
  passedTests++;
} else {
  console.log('✗ Routes array is empty or undefined');
}

// Test 2: Check if main routes exist
totalTests++;
const expectedRoutes = [
  'login',
  '',
  'unauthorized',
  'route-error',
  '**'
];

const routePaths = routes.map(route => route.path);
const hasMainRoutes = expectedRoutes.every(path => routePaths.includes(path));

if (hasMainRoutes) {
  console.log('✓ Main routes exist (login, protected, error pages)');
  passedTests++;
} else {
  console.log('✗ Missing main routes');
  console.log('Expected:', expectedRoutes);
  console.log('Found:', routePaths);
}

// Test 3: Check if protected routes have proper structure
totalTests++;
const protectedRoute = routes.find(route => route.path === '');
if (protectedRoute && protectedRoute.children && protectedRoute.children.length > 0) {
  console.log('✓ Protected routes have proper nested structure');
  passedTests++;
} else {
  console.log('✗ Protected routes missing or improperly structured');
}

// Test 4: Check if lazy loading is configured
totalTests++;
const lazyRoutes = routes.filter(route => 
  route.loadComponent || 
  (route.children && route.children.some(child => child.loadComponent))
);

if (lazyRoutes.length > 0) {
  console.log('✓ Lazy loading is configured for routes');
  passedTests++;
} else {
  console.log('✗ No lazy loading configured');
}

// Test 5: Check if role-based access is configured
totalTests++;
const routesWithRoles = [];
function checkRoutesForRoles(routeArray: any[], parentPath = '') {
  routeArray.forEach(route => {
    const fullPath = parentPath + '/' + (route.path || '');
    if (route.data && route.data.roles) {
      routesWithRoles.push({ path: fullPath, roles: route.data.roles });
    }
    if (route.children) {
      checkRoutesForRoles(route.children, fullPath);
    }
  });
}

checkRoutesForRoles(routes);

if (routesWithRoles.length > 0) {
  console.log('✓ Role-based access control configured');
  console.log('Routes with roles:', routesWithRoles.length);
  passedTests++;
} else {
  console.log('✗ No role-based access control found');
}

// Summary
console.log('\n=== Routing Test Summary ===');
console.log(`Passed: ${passedTests}/${totalTests} tests`);
console.log(`Success Rate: ${Math.round((passedTests / totalTests) * 100)}%`);

if (passedTests === totalTests) {
  console.log('🎉 All routing tests passed!');
} else {
  console.log('⚠️  Some routing tests failed. Check configuration.');
}

// Export for potential use
export const routingTestResults = {
  passed: passedTests,
  total: totalTests,
  successRate: Math.round((passedTests / totalTests) * 100)
};