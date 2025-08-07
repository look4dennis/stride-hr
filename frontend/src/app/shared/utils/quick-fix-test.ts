// Quick Fix Test Utility
// Test the recent fixes for projects and weather

declare global {
  interface Window {
    testFixes: () => void;
  }
}

export function setupQuickFixTest() {
  window.testFixes = () => {
    console.log('🧪 Testing recent fixes...');
    
    // Test 1: Check if weather service shows India
    console.log('1. Testing weather service...');
    const weatherElements = document.querySelectorAll('.weather-location');
    if (weatherElements.length > 0) {
      const locationText = weatherElements[0].textContent;
      console.log('Weather location:', locationText);
      if (locationText?.includes('India') || locationText?.includes('Mumbai')) {
        console.log('✅ Weather shows India location');
      } else {
        console.log('❌ Weather still shows:', locationText);
      }
    }
    
    // Test 2: Check if projects page loads without errors
    console.log('2. Testing projects navigation...');
    const router = (window as any).ng?.getComponent?.(document.querySelector('app-root'))?.router;
    if (router) {
      router.navigate(['/projects']).then((result: boolean) => {
        console.log('Projects navigation result:', result);
        setTimeout(() => {
          const projectElements = document.querySelectorAll('.project-card, .projects-grid');
          if (projectElements.length > 0) {
            console.log('✅ Projects page loaded with content');
          } else {
            console.log('❌ Projects page has no content');
          }
        }, 1000);
      });
    }
    
    // Test 3: Check console for errors
    console.log('3. Monitor console for errors...');
    console.log('Check above for any "Error Code: 200" messages');
    
    console.log('🎯 Fix test completed. Check results above.');
  };
  
  console.log('🔧 Quick fix test loaded! Use testFixes() to test recent fixes.');
}

// Auto-setup
if (typeof window !== 'undefined') {
  setupQuickFixTest();
}