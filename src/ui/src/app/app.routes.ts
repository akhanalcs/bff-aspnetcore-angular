import { Routes } from '@angular/router';

export const routes: Routes = [
    // Until the home page exists (roadmap 4), the app opens on the order list.
    {
        path: '',
        redirectTo: '/repair-orders',
        pathMatch: 'full'
    },
    {
        path: 'repair-orders',
        title: 'Repair Orders',
        // Lazy-loaded: the build puts this page (its .ts, template and CSS) in its own JS chunk.
        // import() makes the browser download that chunk the first time someone opens the page
        // (from ng serve via YARP in dev; from the server's wwwroot in production). It resolves to the
        // module (everything the file exports); .then(m => m.OrderList) picks out the OrderList class.
        // The router then creates an OrderList and shows it after <router-outlet>.
        loadComponent: () => import('./repair-orders/order-list').then(m => m.OrderList),
    }
];
