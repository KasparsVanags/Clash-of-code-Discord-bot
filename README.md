# Clash of Code discord bot
Creates a private Clash of Code lobby by using a Discord slash command, responds to the command with a link and deletes the message after 5 minutes.  
/clash mode language

# Setup
Insert Codingame rememberMe cookie and Discord token inside appconfig.json

### Cookie
Log into https://www.codingame.com/ with an account you'll use for the bot and open dev tools (F12 in Chrome), find "rememberMe" cookie and copy paste the value from value tab  
![image](https://user-images.githubusercontent.com/106081841/192303232-49f774ba-dc12-486a-a643-ef05a516b9a7.png)  
:exclamation:Don't log out of the bot account, if you want to use another account on Codingame after you've set up the bot delete rememberMe cookie from your browser or use incognito mode while setting up the bot.  
:exclamation:Cookie will expire after 1 year and will have to be updated.

### Discord token  
Your Discord token can be found at https://discord.com/developers/applications/ in the bot tab.
