# Info

MenuManager main repository

CSDevs topic: https://csdevs.net/resources/menumanager.726/ (There is all info about buttons, config, demo and etc.)


MenuManagerCore - main core plugin

MenuManagerApi - api lib project

MenuManagerText - just an example how to use this plugin

# Disclaimer

Very often i get questions about move blocking method while MenuWhileOpenMenu is set to false and get requests to change it to modifying MoveType param from current. And every time i decline it. Why? Nothing hard

When player move blocked with modified MoveType he can abuse it to get advantage in match with some unusual positions (while jumping down from short to CTs on de_dust2 for example)

So method with setting VelocityModifier was taken as main one. Another method option could be added in future but i don't make a promise now...
