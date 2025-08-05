// StrideHR Custom Swagger UI JavaScript

(function() {
    'use strict';

    // Wait for Swagger UI to load
    window.addEventListener('load', function() {
        initializeCustomFeatures();
    });

    function initializeCustomFeatures() {
        // Add custom header
        addCustomHeader();
        
        // Add search functionality
        addSearchFunctionality();
        
        // Add copy to clipboard functionality
        addCopyToClipboard();
        
        // Add request/response examples
        addExamples();
        
        // Add keyboard shortcuts
        addKeyboardShortcuts();
        
        // Add dark mode toggle
        addDarkModeToggle();
        
        // Add API status indicator
        addApiStatusIndicator();
        
        // Add custom footer
        addCustomFooter();
    }

    function addCustomHeader() {
        const topbar = document.querySelector('.swagger-ui .topbar');
        if (topbar) {
            const headerInfo = document.createElement('div');
            headerInfo.className = 'custom-header-info';
            headerInfo.innerHTML = `
                <div style="display: flex; justify-content: space-between; align-items: center; color: white; padding: 0 20px;">
                    <div>
                        <h2 style="margin: 0; font-size: 1.5rem;">StrideHR API Documentation</h2>
                        <p style="margin: 5px 0 0 0; opacity: 0.9; font-size: 0.9rem;">Comprehensive HR Management System API</p>
                    </div>
                    <div style="text-align: right;">
                        <div style="font-size: 0.8rem; opacity: 0.8;">Version 1.0</div>
                        <div style="font-size: 0.8rem; opacity: 0.8;">Last Updated: ${new Date().toLocaleDateString()}</div>
                    </div>
                </div>
            `;
            topbar.appendChild(headerInfo);
        }
    }

    function addSearchFunctionality() {
        const infoSection = document.querySelector('.swagger-ui .info');
        if (infoSection) {
            const searchContainer = document.createElement('div');
            searchContainer.className = 'api-search-container';
            searchContainer.innerHTML = `
                <div style="margin: 20px 0; padding: 20px; background: #f8fafc; border-radius: 8px; border: 1px solid #e5e7eb;">
                    <h3 style="margin: 0 0 15px 0; color: #1f2937;">🔍 Search API Endpoints</h3>
                    <input type="text" id="api-search" placeholder="Search endpoints, methods, or descriptions..." 
                           style="width: 100%; padding: 12px; border: 2px solid #d1d5db; border-radius: 6px; font-size: 1rem;">
                    <div id="search-results" style="margin-top: 15px;"></div>
                </div>
            `;
            infoSection.appendChild(searchContainer);

            const searchInput = document.getElementById('api-search');
            const searchResults = document.getElementById('search-results');

            searchInput.addEventListener('input', function(e) {
                const query = e.target.value.toLowerCase();
                if (query.length < 2) {
                    searchResults.innerHTML = '';
                    showAllOperations();
                    return;
                }

                const operations = document.querySelectorAll('.swagger-ui .opblock');
                let matchCount = 0;
                let resultsHtml = '';

                operations.forEach(operation => {
                    const summary = operation.querySelector('.opblock-summary-description');
                    const path = operation.querySelector('.opblock-summary-path');
                    const method = operation.querySelector('.opblock-summary-method');
                    
                    const summaryText = summary ? summary.textContent.toLowerCase() : '';
                    const pathText = path ? path.textContent.toLowerCase() : '';
                    const methodText = method ? method.textContent.toLowerCase() : '';

                    if (summaryText.includes(query) || pathText.includes(query) || methodText.includes(query)) {
                        operation.style.display = 'block';
                        matchCount++;
                        resultsHtml += `
                            <div style="padding: 8px; background: white; margin: 5px 0; border-radius: 4px; border-left: 3px solid #3b82f6;">
                                <strong>${methodText.toUpperCase()}</strong> ${pathText}
                                <br><small style="color: #6b7280;">${summaryText}</small>
                            </div>
                        `;
                    } else {
                        operation.style.display = 'none';
                    }
                });

                searchResults.innerHTML = matchCount > 0 
                    ? `<div style="color: #059669; font-weight: 500;">Found ${matchCount} matching endpoints:</div>${resultsHtml}`
                    : '<div style="color: #ef4444;">No endpoints found matching your search.</div>';
            });
        }
    }

    function showAllOperations() {
        const operations = document.querySelectorAll('.swagger-ui .opblock');
        operations.forEach(operation => {
            operation.style.display = 'block';
        });
    }

    function addCopyToClipboard() {
        // Add copy buttons to code blocks
        const codeBlocks = document.querySelectorAll('.swagger-ui .highlight-code, .swagger-ui .microlight');
        codeBlocks.forEach(block => {
            const copyButton = document.createElement('button');
            copyButton.innerHTML = '📋 Copy';
            copyButton.className = 'copy-button';
            copyButton.style.cssText = `
                position: absolute;
                top: 10px;
                right: 10px;
                background: #3b82f6;
                color: white;
                border: none;
                padding: 5px 10px;
                border-radius: 4px;
                cursor: pointer;
                font-size: 0.8rem;
                z-index: 1000;
            `;

            block.style.position = 'relative';
            block.appendChild(copyButton);

            copyButton.addEventListener('click', function() {
                const text = block.textContent || block.innerText;
                navigator.clipboard.writeText(text).then(() => {
                    copyButton.innerHTML = '✅ Copied!';
                    setTimeout(() => {
                        copyButton.innerHTML = '📋 Copy';
                    }, 2000);
                });
            });
        });
    }

    function addExamples() {
        // Add common request examples
        const tryItOutButtons = document.querySelectorAll('.swagger-ui .try-out__btn');
        tryItOutButtons.forEach(button => {
            button.addEventListener('click', function() {
                setTimeout(() => {
                    addRequestExamples();
                }, 500);
            });
        });
    }

    function addRequestExamples() {
        const textareas = document.querySelectorAll('.swagger-ui .body-param textarea');
        textareas.forEach(textarea => {
            if (textarea.placeholder === '' || !textarea.value) {
                const operation = textarea.closest('.opblock');
                const path = operation.querySelector('.opblock-summary-path').textContent;
                
                // Add example based on endpoint
                if (path.includes('/employees') && textarea.closest('.opblock-post')) {
                    textarea.value = JSON.stringify({
                        "firstName": "John",
                        "lastName": "Doe",
                        "email": "john.doe@company.com",
                        "phone": "+1234567890",
                        "department": "IT",
                        "designation": "Software Engineer",
                        "branchId": 1,
                        "joiningDate": "2024-01-15",
                        "basicSalary": 75000
                    }, null, 2);
                } else if (path.includes('/attendance/checkin')) {
                    textarea.value = JSON.stringify({
                        "location": "Office - Main Building",
                        "coordinates": {
                            "latitude": 40.7128,
                            "longitude": -74.0060
                        },
                        "timestamp": new Date().toISOString()
                    }, null, 2);
                } else if (path.includes('/leave/request')) {
                    textarea.value = JSON.stringify({
                        "leaveType": "Annual",
                        "startDate": "2024-12-15",
                        "endDate": "2024-12-16",
                        "reason": "Personal work",
                        "isHalfDay": false,
                        "emergencyContact": "+1234567890"
                    }, null, 2);
                }
            }
        });
    }

    function addKeyboardShortcuts() {
        document.addEventListener('keydown', function(e) {
            // Ctrl/Cmd + K to focus search
            if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
                e.preventDefault();
                const searchInput = document.getElementById('api-search');
                if (searchInput) {
                    searchInput.focus();
                }
            }
            
            // Escape to clear search
            if (e.key === 'Escape') {
                const searchInput = document.getElementById('api-search');
                if (searchInput && document.activeElement === searchInput) {
                    searchInput.value = '';
                    searchInput.dispatchEvent(new Event('input'));
                    searchInput.blur();
                }
            }
        });

        // Add keyboard shortcuts info
        const infoSection = document.querySelector('.swagger-ui .info');
        if (infoSection) {
            const shortcutsInfo = document.createElement('div');
            shortcutsInfo.innerHTML = `
                <div style="margin: 20px 0; padding: 15px; background: #f0f9ff; border-radius: 8px; border-left: 4px solid #3b82f6;">
                    <h4 style="margin: 0 0 10px 0; color: #1e40af;">⌨️ Keyboard Shortcuts</h4>
                    <div style="font-size: 0.9rem; color: #1f2937;">
                        <strong>Ctrl/Cmd + K</strong> - Focus search box<br>
                        <strong>Escape</strong> - Clear search and close
                    </div>
                </div>
            `;
            infoSection.appendChild(shortcutsInfo);
        }
    }

    function addDarkModeToggle() {
        const topbar = document.querySelector('.swagger-ui .topbar');
        if (topbar) {
            const darkModeToggle = document.createElement('button');
            darkModeToggle.innerHTML = '🌙';
            darkModeToggle.title = 'Toggle Dark Mode';
            darkModeToggle.style.cssText = `
                position: absolute;
                top: 15px;
                right: 20px;
                background: rgba(255, 255, 255, 0.2);
                color: white;
                border: none;
                padding: 8px 12px;
                border-radius: 6px;
                cursor: pointer;
                font-size: 1.2rem;
                transition: all 0.2s ease;
            `;

            darkModeToggle.addEventListener('click', function() {
                document.body.classList.toggle('dark-mode');
                darkModeToggle.innerHTML = document.body.classList.contains('dark-mode') ? '☀️' : '🌙';
                
                // Save preference
                localStorage.setItem('swagger-dark-mode', document.body.classList.contains('dark-mode'));
            });

            // Load saved preference
            if (localStorage.getItem('swagger-dark-mode') === 'true') {
                document.body.classList.add('dark-mode');
                darkModeToggle.innerHTML = '☀️';
            }

            topbar.appendChild(darkModeToggle);
        }
    }

    function addApiStatusIndicator() {
        const topbar = document.querySelector('.swagger-ui .topbar');
        if (topbar) {
            const statusIndicator = document.createElement('div');
            statusIndicator.id = 'api-status';
            statusIndicator.style.cssText = `
                position: absolute;
                top: 50px;
                right: 20px;
                padding: 5px 10px;
                border-radius: 15px;
                font-size: 0.8rem;
                font-weight: 500;
                background: #10b981;
                color: white;
            `;
            statusIndicator.innerHTML = '🟢 API Online';

            // Check API status
            fetch('/health')
                .then(response => {
                    if (response.ok) {
                        statusIndicator.innerHTML = '🟢 API Online';
                        statusIndicator.style.background = '#10b981';
                    } else {
                        statusIndicator.innerHTML = '🟡 API Issues';
                        statusIndicator.style.background = '#f59e0b';
                    }
                })
                .catch(() => {
                    statusIndicator.innerHTML = '🔴 API Offline';
                    statusIndicator.style.background = '#ef4444';
                });

            topbar.appendChild(statusIndicator);
        }
    }

    function addCustomFooter() {
        const swaggerContainer = document.querySelector('.swagger-ui');
        if (swaggerContainer) {
            const footer = document.createElement('div');
            footer.className = 'custom-footer';
            footer.innerHTML = `
                <div style="text-align: center; padding: 30px 20px; color: #6b7280; font-size: 0.875rem; border-top: 1px solid #e5e7eb; margin-top: 40px; background: #f8fafc;">
                    <div style="max-width: 1200px; margin: 0 auto;">
                        <div style="margin-bottom: 15px;">
                            <strong style="color: #1f2937;">StrideHR API Documentation</strong>
                        </div>
                        <div style="display: flex; justify-content: center; gap: 30px; flex-wrap: wrap; margin-bottom: 15px;">
                            <a href="https://docs.stridehr.com" style="color: #3b82f6; text-decoration: none;">📚 Full Documentation</a>
                            <a href="https://status.stridehr.com" style="color: #3b82f6; text-decoration: none;">📊 API Status</a>
                            <a href="mailto:api-support@stridehr.com" style="color: #3b82f6; text-decoration: none;">📧 Support</a>
                            <a href="https://community.stridehr.com" style="color: #3b82f6; text-decoration: none;">💬 Community</a>
                        </div>
                        <div style="font-size: 0.8rem; opacity: 0.8;">
                            © 2024 StrideHR. All rights reserved. | Version 1.0 | Last updated: ${new Date().toLocaleDateString()}
                        </div>
                    </div>
                </div>
            `;
            swaggerContainer.appendChild(footer);
        }
    }

    // Add dark mode styles
    const darkModeStyles = `
        .dark-mode .swagger-ui {
            filter: invert(1) hue-rotate(180deg);
        }
        .dark-mode .swagger-ui img,
        .dark-mode .swagger-ui .topbar {
            filter: invert(1) hue-rotate(180deg);
        }
    `;

    const styleSheet = document.createElement('style');
    styleSheet.textContent = darkModeStyles;
    document.head.appendChild(styleSheet);

})();