# bff-aspnetcore-angular
Implementing a secure web application using nx Standalone Angular and an ASP.NET Core server.

## References
1. [End-to-end security in a web application](https://youtu.be/6cdV-oN_Yao?si=ms_ut_RbGPCvbdNp)
2. https://damienbod.com/2023/09/11/implement-a-secure-web-application-using-nx-standalone-angular-and-an-asp-net-core-server/
3. https://github.com/damienbod/bff-aspnetcore-angular
4. https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2

## Introduction
- There has been a significant shift in how security is implemented for web applications recently.
- Traditionally, SPAs (like those built with Angular) and their backends (like those built with ASP.NET Core) were often implemented using different technical stacks.
- Splitting security between different stacks resulted in sensitive information being exposed in the browser, which is not secure.
- It is now recommended to secure web applications as a single security context.
- This means treating the frontend and backend as a unified entity in terms of security.
- For setups using the Backend for Frontend (BFF) pattern, deploy and secure the frontend (Angular) and backend (ASP.NET Core) together.

- For web applications, use OpenID Connect with a confidential client to connect to any Identity Provider (IdP).
- Follow the IdP's recommendations for a straightforward authentication setup.
- Session protection is relatively simple to implement.
- Authorization is complex and application-specific.
- There are many ways to handle authorization, and each application has unique requirements and business logic.
- Deciding between dynamic and static authorization can be challenging and varies by application.
- While authentication and session protection are straightforward with the right tools, authorization requires careful consideration and is inherently complex.

- The authentication flow should be consistent and well-defined.
- Many architects design the architecture first and add security later, which often leads to problems.
- Security should be integrated into the architecture from the beginning.
- Splitting applications into microservices and separate UIs can complicate security.
- Solutions for issues like downstream API access and single sign-on are needed when using microservices.
- Keeping the architecture simple, with a single web UI and a unified security context, makes security management easier.
- Early architectural decisions significantly impact the ease of securing the application.

## HttpOnly, Secure, and SameSite Cookies
- **HttpOnly:** Prevents JavaScript from accessing the cookie.
- **Secure:** Ensures the cookie is only sent over HTTPS.
- **SameSite:** Prevents the cookie from being sent in a cross-site request.

**HttpOnly** cookies are designed to be inaccessible to JavaScript running in the browser. This includes any JavaScript code, whether it's part of your Angular app or injected by an attacker. The primary purpose of HttpOnly cookies is to protect sensitive information from being accessed through client-side scripts, thereby mitigating the risk of XSS attacks.
When your Angular app makes HTTP requests to your ASP.NET Core backend, it does not need to directly access the HttpOnly cookie. 
Instead, the browser automatically includes the HttpOnly cookie in the HTTP request headers when making requests to the backend. 
When the backend receives a request, it reads the HttpOnly cookie from the request headers to identify the user session.

### Example flow for SameSite, HttpOnly and Secure Cookies
1. **User logs into `myapp.com`**: A session cookie is set with attributes `SameSite=Lax`, `HttpOnly`, and `Secure`.
    - **Cookie Attributes**:
        - **SameSite=Lax**: Ensures the cookie is only sent with same-site requests or top-level navigations.
        - **HttpOnly**: Prevents JavaScript from accessing the cookie.
        - **Secure**: Ensures the cookie is only sent over HTTPS connections.
2. If the user tries to access `http://myapp.com` (an insecure connection), the session cookie will not be sent because of the `Secure` attribute. This protects the cookie from being intercepted by attackers on insecure networks.
3. **User visits `malicious.com`**: The user navigates to a malicious site.
4. **Malicious site includes a script**:
   ```html
   <script>
       // Attempt to read cookies
       console.log(document.cookie);
   </script>
   ```
    - **HttpOnly Behavior**: The `HttpOnly` attribute prevents this script from accessing the session cookie. The script cannot read or manipulate the cookie, protecting it from being stolen via XSS attacks.
5. **Malicious site includes a form**:
   ```html
   <form action="https://myapp.com/transfer" method="POST">
       <input type="hidden" name="amount" value="1000">
       <input type="submit" value="Transfer Money">
   </form>
   ```
6. **User submits the form**: The form submission makes a POST request to `https://myapp.com/transfer`.
7. **Request Origin**: The request originates from `malicious.com`.
8. **Browser Behavior**: The browser sees that the request is cross-site (from `malicious.com` to `myapp.com`) and does not send the `SameSite=Lax` cookie.
9. **Server Response**: Without the session cookie, `myapp.com` does not recognize the user and rejects the request.
10. **Angular app makes an API request**:
    ```typescript
    this.http.get('https://myapp.com/api/user-profile').subscribe(response => {
        console.log(response);
    });
    ```
    - **Browser Behavior**:
      - The browser automatically includes the HttpOnly cookie in the request headers when making the API call to `https://myapp.com/api/user-profile`.
      - The cookie is included in the HTTP request headers by the browser, not by JavaScript. This means that while JavaScript cannot read or manipulate the cookie, it is still sent with requests to the backend.
    - **Backend processes the request**: The ASP.NET Core backend reads the HttpOnly cookie from the request headers to identify the user session and returns the appropriate response.
    - **Response received by Angular app**: The Angular app receives the response and processes it as needed.

### **SameSite=Lax vs. SameSite=Strict**
**SameSite=Lax** example:
1. **User logs into `myapp.com`**: A session cookie is set with `SameSite=Lax`.
2. **User visits `google.com`**: The user navigates to an external site.
3. **User clicks a link back to `myapp.com`**: The session cookie is sent with the request, allowing the user to stay logged in.

**SameSite=Strict** example:
1. **User logs into `myapp.com`**: A session cookie is set with `SameSite=Strict`.
2. **User visits `google.com`**: The user navigates to an external site.
3. **User clicks a link back to `myapp.com`**: The session cookie is **not** sent with the request, so the user may need to log in again.


