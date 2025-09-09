/* Motoreto PWA SW */
const CACHE = 'motoreto-v1';
const PRECACHE_URLS = [
    '/',                // صفحه اصلی
    '/offline.html',    // صفحه آفلاین
    '/Content/style.css?v=4.6',
    '/Content/css/nav.css',
    '/Content/js/main.js',
    '/images/sadr_logo.png'
];

self.addEventListener('install', (event) => {
    event.waitUntil(
        caches.open(CACHE).then((c) => c.addAll(PRECACHE_URLS)).then(() => self.skipWaiting())
    );
});

self.addEventListener('activate', (event) => {
    event.waitUntil(
        caches.keys().then(keys =>
            Promise.all(keys.filter(k => k.startsWith('motoreto-') && k !== CACHE).map(k => caches.delete(k)))
        ).then(() => self.clients.claim())
    );
});

// ناوبری‌ها: شبکه-اول با فالبک آفلاین
self.addEventListener('fetch', (event) => {
    const req = event.request;

    // صفحات (HTML)
    if (req.mode === 'navigate') {
        event.respondWith((async () => {
            try {
                const res = await fetch(req);
                const cache = await caches.open(CACHE);
                cache.put(req, res.clone());
                return res;
            } catch (e) {
                const cached = await caches.match(req);
                return cached || caches.match('/offline.html');
            }
        })());
        return;
    }

    // استاتیک‌ها: stale-while-revalidate
    const url = new URL(req.url);
    const isSameOrigin = url.origin === self.location.origin;
    const isAsset = /\.(css|js|png|jpg|jpeg|webp|svg|woff2?)$/i.test(url.pathname);

    if (isSameOrigin && isAsset) {
        event.respondWith((async () => {
            const cached = await caches.match(req);
            const fetchPromise = fetch(req).then((res) => {
                const copy = res.clone();
                event.waitUntil(
                    caches.open(CACHE).then(c => c.put(req, copy))
                );
                return res;
            }).catch(() => cached);
            return cached || fetchPromise;
        })());
    }
});