# Google Groups Manager 
If your company or organization uses Google Workspace, then you might be frustrated with the fact that workspace admins or members of an email group are the only people that can manage that group. You might think that you could just make the user that needs to manage a group a workspace admin with permission to just manage groups. But what if you don't want them to have access to all groups? It is currently not possible to do this natively in Google Workspace. 
This application is designed to allow users that are not Google Workspace admins to manage email groups without having to be part of the group themselves. 
There are four user permission levels

   **Viewer** - Can view the members of a group

   **Editor** - All permissions of ‘Viewer’ + Can add, edit, and remove group members

   **Admin** - All Permissions of ‘Editor’ + Can add or remove Viewers, Editors, and Admins

   **Super Admin** - All permissions, including creating and deleting groups

# Getting the application running

Docker compose makes it very easy to get this application up and running. Here is a sample docker-compose configuration:

    version: '3.4'
    
    networks:
      dev:
        driver: bridge
    
    services:
      GoogleGroups:
        image: googlegroups:latest
        build:
          context: .
          dockerfile: Dockerfile
        ports:
          - "80:80"
          - "443:443"
        environment:
          - ASPNETCORE_ENVIRONMENT=Release
          - ASPNETCORE_URLS=https://+:443;http://+:80
          - ASPNETCORE_Kestrel__Certificates__Default__Password=<Certificate Password>
          - ASPNETCORE_Kestrel__Certificates__Default__Path=/app/wwwroot/Data/sslCert.pfx
        volumes:
          - <path to local folder>:/app/wwwroot/Data     #Map folder in container to folder on host
        networks:
          - dev
        restart: always
      
      Redis:
        image: redis
        ports:
          - 5002:6379
        networks:
          - dev
        restart: always
**Replace anything inside <> with the appropriate values:** \
**\<Certificate Password\>** - This is the SSL certificate password.\
**\<path to local folder\>** - This is the path to the local folder where data will be stored.

# Setup

Creating a Google Cloud project for managing Google Groups involves several steps, including setting up the project, enabling necessary APIs, and configuring authentication. Here’s a detailed guide to help you through the process:

1. **Create a New Google Cloud Project**  
   1. **Access the Google Cloud Console:**  
      * Go to the Google Cloud Console ([https://console.cloud.google.com/](https://console.cloud.google.com/)).  
      * Sign in with your Google account. Make sure to sign in with an admin account for the domain that you are managing.  
   2. **Create a New Project:**  
      * Click on the project dropdown in the top navigation bar (it may say "Select a project").  
      * Click on "New Project."  
      * Fill in the project name and organization.  
      * Click "Create."  
2. **Enable APIs**  
   1. **Enable the Google Groups API:**  
      * In the Cloud Console, ensure your new project is selected.  
      * Navigate to the *APIs & Services* \> *Library*.  
      *  In the search bar, type “Admin SDK” and select it.  
      * Click “Enable” to activate the API.  
      * Repeat for *Contacts API* and *People API*  
          
3. **Configure OAuth Consent Screen**  
   1. **Set Up OAuth Consent:**  
      * Navigate to *APIs & Services* \> *OAuth consent screen*.  
      * Choose “Internal” as the User type. Fill in the application name, support email, and any other required fields.  
      * Upload a logo.  
      * Set Authorized domain as your Google Workspace Domain name  
      * Click “Save and Continue.”  
      * On the *Scopes* page click on “Add or Remove Scopes”  
      * Enter [https://www.googleapis.com/auth/userinfo.profile](https://www.googleapis.com/auth/userinfo.profile) into the “Manually add scopes” field.  
      * Click “Add to Table” and then “Update”  
      * Click “Save and Continue”  
      * Verify the settings and then click “Back to Dashboard”  
          
4. **Create Credentials**  
   1. **Create OAuth 2.0 Credentials:**  
      * Navigate to *APIs & Services* \> *Credentials*.  
      * Click on “Create Credentials” and select “OAuth client ID.”  
      * Choose “Web application” as the application type.  
      * Fill in the required fields, including the authorized redirect URIs.  
      * Click “Create” and make sure to save your Client ID and Client Secret.  
   2. **Service Account:**  
      * Click on “Create Credentials” and select “Service account.”  
      * Give it a name and click “Create and Continue”  
      * Grant the service account access to your project. In the “Role” dropdown select “Owner”.  
      * Click “Continue” and then “Done”  
      * Click on the service account and make a note of the email and unique ID  
      * Go to the *Keys* tab and select “Add Key” \> “Create new key”  
      * Select “P12” as the Key type and click “Create”.   
      * Enter a password and save the P12 file to your computer.  
5. **Manage Group Permissions**  
   1. **Domain-Wide Delegation (for Service Accounts):**  
      * Go to the Google Admin Console (admin.google.com).  
      * Navigate to *Security* \> *Access and data control* \> *API controls* \> *Manage domain-wide delegation*.  
      * Click “Add new” and enter the Client ID of your service account.  
      * Add the following scopes:  
        * [https://www.googleapis.com/auth/admin.directory.group](https://www.googleapis.com/auth/admin.directory.group)  
        * [https://www.googleapis.com/auth/contacts](https://www.googleapis.com/auth/contacts)  
        * [https://www.googleapis.com/auth/directory.readonly](https://www.googleapis.com/auth/directory.readonly)  
        * [https://www.googleapis.com/auth/contacts.other.readonly](https://www.googleapis.com/auth/contacts.other.readonly)  
        * [https://www.googleapis.com/auth/admin.directory.domain](https://www.googleapis.com/auth/admin.directory.domain)  
        * [https://www.googleapis.com/auth/admin.directory.user](https://www.googleapis.com/auth/admin.directory.user)
          



