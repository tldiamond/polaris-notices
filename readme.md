Getting Started
---------------

* Copy the user.config.example file and rename to user.config
* Fill in the required fields in the user.config file
  * The database connections use integrated security, so you'll want to make sure that whatever account you use to run the application pool has the necessary database permissions
* Open the two resources files in /Resources and populate them with the **Parent Organization** Abbreviation and Recordset ID for each mail event
  * This is the way the application is currently set up to work, but could easily be tweaked to just dump everything into one giant recordset if desired
* Build the project to make sure that all the nuget packages are pulled in properly
* Publish and test


Testing
-------
* Email bounces
  * `https://[your url]/Email/TestBounce?Auth=[your auth string]&email=[your test email account] `
* Email spam reports
  * `https://[your url]/Email/TestSpam?Auth=[your auth string]&email=[your test email account] `
* Sending SMS
  * Make sure your Twilio credentials and SMS test phone number are supplied in the user.config
    * `https://[your url]/Sms/Test?auth=[your auth string] `