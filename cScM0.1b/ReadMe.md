cScM - C# Cryptopia Manager 0.1b

Requires	- .NET 4.5 to run
and	- Newtonsoft.Json for coders using API.cs

DISCLAIMER
========
This product is provided as-is, free to use and is provided without warranty.  Some of it's functions submit live trades, transfers, tips and withdrawals.
Errors can result in the loss of cryptocurrency.  Although I am taking every effort to ensure this applciation works perfectly with the API, things are
subject to change without you or I receiving notice.  All input data **especially** programmatically via API.cs. should be checked thoroughly.  By using
cScM, you assume all liability and responsibility associated.

INFO
====
Cryptopia Manager provides a desktop interface to all public Currency and Market data as well as Private Balances, Orders, Trades and Transactions.
The WebUI at cryptopia.co.nz now requires 2FA on web login, which I agree is a wise measure for such valuable assets. However the desktop interface
provides users a means to perform daily trades via a traditional Name and Password.  This should lead to a smoother daily experience and less accounts
being locked out.  Local API keys are stored under SHA-256 encryption.

HOW TO USE
=========
In short, provide the application with API Keys which can then be saved or loaded, alternatively you can use the session menu to get everything public.
Although API Keys are not required to used this application, it is recommended that you link it to your account to access all available features.
I have provided a very brief and easy to read manual for a more in depth introduction to the application.

TROUBLESHOOTING
==============
This application is in it's infancy, and although I have taken every step to assure API communications are smooth and accurate.  In the event of errors,
I recommend that users enable both API and DEBUG loggin in the settings menu before triggering the error again.  This information in the log files may
be used to resolve any errors I may not have anticipated.  These can then be brought to my attention via the GIT repository or the Cryptopia forums.