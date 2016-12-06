#Disclaimer
This application has been primarily used and tested against version 5.0. This application comes with no guarantee and its use is solely at your own risk. I take no responsibility for anything that may happen to your database as a result of the use of this software. While setting up and configuring the system you should take caution to not point it at your production server and instead point to a well backed up training or development server instead.

#Setup & Config

##Email Notice Handling
This portion does **NOT** require the Polaris API. This is an application to enable better notice handling for the Polaris ILS. It lets you automate the handling of bounced email messages and spam reports. When one of these events is triggered the application will take the following steps:

* Add all patron records using that email address to a record set, based upon the **Parent Organization** of that patron's registered library
  * If you have library specific spam processing enabled only patrons registered at the library that sent the notice will be processed
* Change their delivery option from email to phone, if they have one, or to mail otherwise
* Remove the email address from their account, from either the regular or alternate email address field
* Add a non-blocking note to their account with the date, the old email address, and a brief description of why the address was removed.

##Create Notices Active Directory User
This is the account you'll give database permissions to and use to run the application pool

##Database Setup
Run create_notices_db.sql from Extras folder in src

##Mandrill Setup
###Create Webhook
* Set up a webhook to trigger on hard_bounce, soft_bounce, spam, unsub, and reject that posts to `https://[your url]/email/processfailure?auth=[your auth string]`

###Example Mandril Setup
![Example Mandrill Setup](http://media.virbcdn.com/files/87/cdb3b170746c80cc-webhook_setup.png "Example Mandrill Setup")

###SMS Configuration and Other General Information
* You can find Wes's slides from PUG 2014 with more setup information at http://media.virbcdn.com/files/17/6b7afe76865df50e-CloudPUG2014.pdf

##Application Configuration
These steps are from Visual Studio 2015, depending on your version and environment some adjustments may be necessary.
* Open the solution in Visual Studio and do a build to make sure all the nuget packages get installed properly
* Fill in the required fields in the settings.config file
  * auth_string
  * permission_admin_group
  * db_server
  * mandrill_api_key
  * enable_email_processing
  * library_specific_spam
* Add an entry to CLC_Notices.dbo.RecordSets for each **library ID** you'd like to process for both spam and bounce record sets
  *  Each library can share the same record set but they must be set up individually
  *  It should be fine pointing both spam and bounce to the same record set but I've never tested it
* If using library specific spam reporting add an entry to the CLC_Notices.dbo.EmailDomains table for each domain you use to send emails. This table expects a **library ID**. 
* Publish the project to your IIS web server
* Configure the application pool to use Windows domain credentials with database access
* Test

##Testing
* Email bounces
  * `https://[your url]/Email/TestBounce?Auth=[your auth string]&email=[your test email account]`
* Email spam reports
  * `https://[your url]/Email/TestSpam?Auth=[your auth string]&email=[your test email account]&from=[your notice from account]`
    * The `from` querystring is optional but useful for testing the library specific spam option
