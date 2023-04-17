# LeagueJunction

Organisation tool for League Of DAE Legends tournaments. League of DAE Legends is a student club for students currently enrolled in the DAE ([Digital Arts & Entertainment](https://www.digitalartsandentertainment.be/)) curriculum, DAE veterans and ex-DAE students. As the name implies, this student club focuses on League of Legends. 

This tool is looking to automate the process from students registering to participate to making teams and all the way to tracking how players are performing to be able to forge a scoreboard.

In short, **LeagueJunction** has 3 topics:
- Balancing (Processing registrations and making reasonably balanced teams)
- Tournaments (Generating match keys through the Riot API)
- Personal progression (Database that keeps track of individual players their performance)

## Packages and APIs

List of used packages and APIs and links to documentation of those APIs to make it easier for developers to contibute or make changes for their own needs.

- [Discord.NET](https://discordnet.dev/api/index.html)
- [NewtonSoft.JSON](https://www.newtonsoft.com/json/help/html/Introduction.htm)
- [CommunityToolkit.MVVM](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [CsvHelper by Josh Close](https://joshclose.github.io/CsvHelper/getting-started/)
- [Riot API](https://developer.riotgames.com/apis)

## Our workflow

This tool started being formed because the need arised to automate our current workflow.
Our registrations happen through [Google Forms](https://www.google.com/intl/nl_be/forms/about/) where players enter their Discord username, main account op.gg link, region account op.gg link (international students often have an account they play on for lower ping which does not represent their true rank) and preferred roles (top, jngl, mid, ...).
We extract 2 important pieces of information from our registration, the **username** and **region** which are essential for our automatisation. With these pieces of information we can pull all necessary data like ranks.
Optional data that we also extract preferred roles, display name (username they will play with) and Discord username.

Next up we fill in all missing data through [Riot API](https://developer.riotgames.com/apis), mainly the rank.
Based on this rank we calculate some kind of approximate MMR value for our next step.
The next step is making teams. We try to make them somewhat balanced by getting the best and worst players together. We use our aproximate MMR value for this.

When all the teams are done, all that is left to do is posting them to Discord, which we do through a [webhook](https://support.discord.com/hc/en-us/articles/228383668-Intro-to-Webhooks). We opted for a webhook instead of a bot since maintenance, upkeep and development costs are significantly lower. Besides, we don't really need the extra features a bot offers.

## Custom repository

If you would like our features but want to change the input method (a json instead of csv or a different kind of Google Forms) all you have to do is implement the `IPlayerRepository` interface and instantiate the repository in the `BalanceVM` (Balance viewmodel). 