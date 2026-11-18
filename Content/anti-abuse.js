// Anti-Abuse System - Frontend Helper
// Supports: Device Fingerprinting, Debouncing, Cooldown, CAPTCHA

(function(window) {
    'use strict';

    var AntiAbuse = {
        // Configuration
        config: {
            cooldownSeconds: 60,
            debounceMs: 300,
            maxAttempts: 5,
            fpCookieName: 'device-fp'
        },

        // State
        state: {
            lastSubmitTime: 0,
            submitting: false,
            cooldownActive: false
        },

        /**
         * Initialize anti-abuse system
         */
        init: function(options) {
            Object.assign(this.config, options || {});
            this.generateAndStoreFingerprint();
            this.setupFormHandlers();
            return this;
        },

        /**
         * Generate device fingerprint from browser headers
         * Stored in localStorage for reuse
         */
        generateAndStoreFingerprint: function() {
            var existingFp = this.getFingerprint();
            if (existingFp) return existingFp;

            // Create fingerprint from available data
            var fp = this.hashString(
                navigator.userAgent +
                '|' + (navigator.language || navigator.userLanguage) +
                '|' + screen.width +
                '|' + screen.height +
                '|' + new Date().getTimezoneOffset()
            );

            // Store in localStorage
            try {
                localStorage.setItem(this.config.fpCookieName, fp);
            } catch (e) {
                // Fallback if localStorage not available
                console.warn('localStorage not available for device fingerprint');
            }

            return fp;
        },

        /**
         * Get stored fingerprint
         */
        getFingerprint: function() {
            try {
                return localStorage.getItem(this.config.fpCookieName);
            } catch (e) {
                return null;
            }
        },

        /**
         * Setup form handlers for debouncing and cooldown
         */
        setupFormHandlers: function() {
            var self = this;

            // Debounce form submissions
            $('form').on('submit', function(e) {
                if (self.state.submitting || self.state.cooldownActive) {
                    e.preventDefault();
                    self.showMessage('Please wait before trying again.', 'warning');
                    return false;
                }

                self.state.submitting = true;

                // Re-enable after debounce period
                setTimeout(function() {
                    self.state.submitting = false;
                }, self.config.debounceMs);
            });

            // Disable double-click on buttons
            $(document).on('click', 'button[type="submit"], input[type="submit"]', function() {
                $(this).prop('disabled', true);
                $(this).data('original-text', $(this).text());
                $(this).text('Processing...');

                setTimeout(function() {
                    // Re-enable after response (should be fast for local validation)
                }, 1000);
            });
        },

        /**
         * Start cooldown timer
         */
        startCooldown: function(seconds) {
            var self = this;
            self.state.cooldownActive = true;

            var remaining = seconds || self.config.cooldownSeconds;
            var $cooldownEl = $('#cooldown-timer');

            if (!$cooldownEl.length) {
                $cooldownEl = $('<div id="cooldown-timer" class="alert alert-info"></div>')
                    .insertAfter('form');
            }

            var interval = setInterval(function() {
                $cooldownEl.text('Please wait ' + remaining + ' seconds before trying again.');

                remaining--;
                if (remaining < 0) {
                    clearInterval(interval);
                    $cooldownEl.remove();
                    self.state.cooldownActive = false;
                }
            }, 1000);
        },

        /**
         * Check if form submission is allowed (rate limit check)
         */
        canSubmit: function() {
            if (this.state.cooldownActive) {
                return false;
            }

            var now = new Date().getTime();
            var timeSinceLastSubmit = now - this.state.lastSubmitTime;

            if (timeSinceLastSubmit < this.config.debounceMs) {
                return false;
            }

            this.state.lastSubmitTime = now;
            return true;
        },

        /**
         * Show message to user
         */
        showMessage: function(message, type) {
            type = type || 'info';
            var alertClass = 'alert-' + type;

            var $alert = $('<div class="alert ' + alertClass + ' alert-dismissible">')
                .html(message)
                .prepend('<button type="button" class="close" data-dismiss="alert">&times;</button>');

            $('form').before($alert);

            // Auto-dismiss after 5 seconds
            setTimeout(function() {
                $alert.fadeOut(function() { $(this).remove(); });
            }, 5000);
        },

        /**
         * Simple hash function for fingerprint
         */
        hashString: function(str) {
            var hash = 0;
            for (var i = 0; i < str.length; i++) {
                var char = str.charCodeAt(i);
                hash = ((hash << 5) - hash) + char;
                hash = hash & hash; // Convert to 32-bit integer
            }
            return Math.abs(hash).toString(36);
        },

        /**
         * Track analytics event (optional)
         */
        trackEvent: function(eventName, data) {
            console.log('[AntiAbuse]', eventName, data);
            // Can integrate with analytics service here
        }
    };

    // Export to global scope
    window.AntiAbuse = AntiAbuse;

})(window);

// Auto-initialize on document ready
$(document).ready(function() {
    if (window.AntiAbuse) {
        window.AntiAbuse.init({
            cooldownSeconds: 60,
            debounceMs: 300
        });
    }
});
