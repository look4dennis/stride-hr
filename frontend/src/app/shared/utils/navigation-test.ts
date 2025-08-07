/**
 * Navigation Test Utilities
 * Test browser back/forward button functionality
 */

export function setupNavigationTest() {
  if (typeof window === 'undefined') return;

  // Add test functions to window object for console testing
  (window as any).testNavigation = {
    // Test basic navigation
    testBasicNavigation: () => {
      console.log('🧪 Testing basic navigation...');
      
      // Navigate to different routes
      const routes = ['/dashboard', '/employees', '/attendance', '/projects'];
      let currentIndex = 0;
      
      const navigateNext = () => {
        if (currentIndex < routes.length) {
          const route = routes[currentIndex];
          console.log(`📍 Navigating to: ${route}`);
          window.location.hash = route;
          currentIndex++;
          setTimeout(navigateNext, 1000);
        } else {
          console.log('✅ Basic navigation test complete. Try using browser back/forward buttons now.');
        }
      };
      
      navigateNext();
    },

    // Test browser back button
    testBackButton: () => {
      console.log('🧪 Testing browser back button...');
      console.log('📍 Current URL:', window.location.href);
      console.log('🔙 Going back...');
      window.history.back();
      
      setTimeout(() => {
        console.log('📍 New URL after back:', window.location.href);
        console.log('✅ Back button test complete');
      }, 500);
    },

    // Test browser forward button
    testForwardButton: () => {
      console.log('🧪 Testing browser forward button...');
      console.log('📍 Current URL:', window.location.href);
      console.log('🔜 Going forward...');
      window.history.forward();
      
      setTimeout(() => {
        console.log('📍 New URL after forward:', window.location.href);
        console.log('✅ Forward button test complete');
      }, 500);
    },

    // Test navigation state
    testNavigationState: () => {
      console.log('🧪 Testing navigation state...');
      console.log('📊 History length:', window.history.length);
      console.log('📍 Current URL:', window.location.href);
      console.log('🔗 Current pathname:', window.location.pathname);
      console.log('🔗 Current hash:', window.location.hash);
      
      // Test if we can go back
      const canGoBack = window.history.length > 1;
      console.log('🔙 Can go back:', canGoBack);
      
      console.log('✅ Navigation state test complete');
    },

    // Test popstate event
    testPopstateEvent: () => {
      console.log('🧪 Testing popstate event handling...');
      
      let popstateCount = 0;
      const popstateHandler = (event: PopStateEvent) => {
        popstateCount++;
        console.log(`🎯 Popstate event #${popstateCount}:`, {
          state: event.state,
          url: window.location.href,
          pathname: window.location.pathname
        });
      };
      
      window.addEventListener('popstate', popstateHandler);
      
      console.log('👂 Popstate listener added. Navigate using browser buttons to see events.');
      console.log('🛑 Run testNavigation.stopPopstateTest() to remove listener');
      
      (window as any).testNavigation.stopPopstateTest = () => {
        window.removeEventListener('popstate', popstateHandler);
        console.log('🛑 Popstate listener removed');
        console.log(`📊 Total popstate events captured: ${popstateCount}`);
      };
    },

    // Test keyboard shortcuts
    testKeyboardShortcuts: () => {
      console.log('🧪 Testing keyboard shortcuts...');
      console.log('⌨️  Alt + Left Arrow = Go Back');
      console.log('⌨️  Alt + Right Arrow = Go Forward');
      console.log('🎯 Try the keyboard shortcuts now!');
      
      const keyHandler = (event: KeyboardEvent) => {
        if (event.altKey && event.key === 'ArrowLeft') {
          console.log('🔙 Alt + Left detected - Going back');
          event.preventDefault();
          window.history.back();
        }
        if (event.altKey && event.key === 'ArrowRight') {
          console.log('🔜 Alt + Right detected - Going forward');
          event.preventDefault();
          window.history.forward();
        }
      };
      
      document.addEventListener('keydown', keyHandler);
      
      console.log('👂 Keyboard listener added');
      console.log('🛑 Run testNavigation.stopKeyboardTest() to remove listener');
      
      (window as any).testNavigation.stopKeyboardTest = () => {
        document.removeEventListener('keydown', keyHandler);
        console.log('🛑 Keyboard listener removed');
      };
    },

    // Run all tests
    runAllTests: () => {
      console.log('🚀 Running all navigation tests...');
      console.log('=====================================');
      
      (window as any).testNavigation.testNavigationState();
      console.log('');
      
      setTimeout(() => {
        (window as any).testNavigation.testPopstateEvent();
        console.log('');
      }, 1000);
      
      setTimeout(() => {
        (window as any).testNavigation.testKeyboardShortcuts();
        console.log('');
      }, 2000);
      
      setTimeout(() => {
        (window as any).testNavigation.testBasicNavigation();
      }, 3000);
    },

    // Clean up all tests
    cleanup: () => {
      console.log('🧹 Cleaning up navigation tests...');
      
      if ((window as any).testNavigation.stopPopstateTest) {
        (window as any).testNavigation.stopPopstateTest();
      }
      
      if ((window as any).testNavigation.stopKeyboardTest) {
        (window as any).testNavigation.stopKeyboardTest();
      }
      
      console.log('✅ Navigation test cleanup complete');
    }
  };

  console.log('🧪 Navigation test utilities loaded!');
  console.log('📋 Available commands:');
  console.log('  testNavigation.testBasicNavigation()');
  console.log('  testNavigation.testBackButton()');
  console.log('  testNavigation.testForwardButton()');
  console.log('  testNavigation.testNavigationState()');
  console.log('  testNavigation.testPopstateEvent()');
  console.log('  testNavigation.testKeyboardShortcuts()');
  console.log('  testNavigation.runAllTests()');
  console.log('  testNavigation.cleanup()');
}

// Auto-setup when imported
if (typeof window !== 'undefined') {
  setupNavigationTest();
}