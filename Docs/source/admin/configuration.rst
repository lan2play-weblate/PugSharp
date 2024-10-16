Configuration
==================================================

Introduction
----------------------------------------
There are two types of configurations that are relevant for PugSharp. 


PugSharp Configs
----------------------------------------

The configs for PugSharp wich are located in the subfolder ``PugSharp/Config`` on your cs2 server.

Matchconfig
........................
The matchconfig defines the current match. It is not loaded automatically, so you have to do that with one of the :ref:`admin/commands:Admin/Rcon Commands` ``ps_loadconfig`` or ``ps_loadconfigfile`` .

Matchconfig Fields
'''''''''''''''''''''
+--------------------------+-----------------+-------------------------------------------------------------------------------------------+
|          Field           |  DefaultValue   |                                        Description                                        |
+==========================+=================+===========================================================================================+
| maplist                  | none (required) | List of available maps for the map vote                                                   |
+--------------------------+-----------------+-------------------------------------------------------------------------------------------+
| team1                    | none (required) | Team declaration                                                                          |
+--------------------------+-----------------+-------------------------------------------------------------------------------------------+
| team2                    | none (required) | Team declaration                                                                          |
+--------------------------+-----------------+-------------------------------------------------------------------------------------------+
| matchid                  | none (required) | Unique Identifier for the match                                                           |
+--------------------------+-----------------+-------------------------------------------------------------------------------------------+
| num_maps                 | 1               | Number of Maps to be played. This should be an odd number to determine a winner.          |
+--------------------------+-----------------+-------------------------------------------------------------------------------------------+
| players_per_team         | 5               | Maximum possible number of players per team.                                              |
+--------------------------+-----------------+-------------------------------------------------------------------------------------------+
| min_players_to_ready     | 5               | Number of players per team that have to be ready to start the game.                       |
+--------------------------+-----------------+-------------------------------------------------------------------------------------------+
| max_rounds               | 24              | Maximum number of rounds played for the main match.                                       |
+--------------------------+-----------------+-------------------------------------------------------------------------------------------+
| max_overtime_rounds      | 6               | Maximum number of rounds played in overtime.                                              |
+--------------------------+-----------------+-------------------------------------------------------------------------------------------+
| vote_timeout             | 60000 (60s)     | Timeout in milliseconds. If a team does not complete the vote within this timeout, the    |
|                          |                 | map with the most votes gets banned.                                                      |
+--------------------------+-----------------+-------------------------------------------------------------------------------------------+
| eventula_apistats_url    | (optional)      | URL where the Game State has to be sent.                                                  |
+--------------------------+-----------------+-------------------------------------------------------------------------------------------+
| eventula_apistats_token  | (optional)      | Optional AuthToken used to authenticate on apistats upload.                               |
+--------------------------+-----------------+-------------------------------------------------------------------------------------------+
| eventula_demo_upload_url | (optional)      | URL to upload the game demo to `eventula <https://github.com/Lan2Play/eventula-manager>`_ |
+--------------------------+-----------------+-------------------------------------------------------------------------------------------+
| g5_api_url               | (optional)      | URL to send the g5 events to.                                                             |
+--------------------------+-----------------+-------------------------------------------------------------------------------------------+
| g5_api_header            | (optional)      | Header that should be set to access the g5 events API.                                    |
+--------------------------+-----------------+-------------------------------------------------------------------------------------------+
| g5_api_headervalue       | (optional)      | Header value that should be set to access the g5 events API.                              |
+--------------------------+-----------------+-------------------------------------------------------------------------------------------+
| allow_suicide            | true            | Flag to determine if players are allowed to suicide.                                      |
+--------------------------+-----------------+-------------------------------------------------------------------------------------------+
| vote_map                 | de_dust2        | Map used during warm-up and voting.                                                       |
+--------------------------+-----------------+-------------------------------------------------------------------------------------------+
    
Matchconfig Example
'''''''''''''''''''''

.. code-block:: json

   {
     "maplist": ["de_vertigo", "de_dust2", "de_inferno", "de_mirage", "de_nuke", "de_overpass", "de_ancient"],
     "team1": {
       "name": "hallo",
       "tag": "hallo",
       "flag": "DE",
       "players": {
         "12345678901234567": "Apfelwurm",
         "12345678901234568": "strange name"
       }
     },
     "team2": {
       "name": "asd",
       "tag": "asd",
       "flag": "DE",
       "players": {
         "12345678901234569": "BOT R00st3r",
         "76561198064576360": "heatwave"
       }
     },
     "matchid": "40",
     "num_maps": 1,
     "players_per_team": 2,
     "min_players_to_ready": 2,
     "max_rounds": 24,
     "max_overtime_rounds": 6,
     "vote_timeout": 60000,
     "eventula_apistats_url": "https://dev.lan2play.de/api/matchmaking/40/",
     "eventula_apistats_token": "Bearer S0XRU0UhIExFQ0tFUiEK",
     "eventula_demo_upload_url": "https://dev.lan2play.de/api/matchmaking/40/demo",
     "vote_map": "de_inferno"
   }

Serverconfig
........................
The Serverconfig defines server wide PugSharp settings for your server. It is loaded automatically when PugSharp is loaded.

Serverconfig Fields
'''''''''''''''''''''
+--------+--------------------------------------------------------------------------------------------+
| Field  |                                        Description                                         |
+========+============================================================================================+
| admins | List of admins with the steamId and a                                                      |
|        | Name (enables the execution of the :ref:`admin/commands:Admin/Rcon Commands` for a player) |
+--------+--------------------------------------------------------------------------------------------+

Serverconfig Example
'''''''''''''''''''''

.. code-block:: json

   {
     "admins": {
       "12345678901234569": "BOT R00st3r",
       "12345678901234567": "Apfelwurm"
     }
   }



CS2 Server Configs
----------------------------------------

The sample configs for the CS2 Server itself wich are located in the subfolder ``cfg/PugSharp`` on your cs2 server.

+------------+--------------------------------------------------------------------------------+
|   Config   |                            Execution point in time                             |
+============+================================================================================+
| warmup.cfg | this config is loaded on every warmup in the game (pre Vote/ Vote / pre ready) |
+------------+--------------------------------------------------------------------------------+
| live.cfg   | this config is loaded on the start of the actual game                          |
+------------+--------------------------------------------------------------------------------+