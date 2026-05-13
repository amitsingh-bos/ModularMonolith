// graphbuilder generated Cypher
MERGE (meta:GraphMeta { graphModel: 'API_v1', schemaVersion: 'v1' });

MERGE (n:Controller { key: 'controller:ModularMonolith.Controllers.WeatherForecastController', graphModel: 'API_v1' })
SET n += { `key`: 'controller:ModularMonolith.Controllers.WeatherForecastController', `name`: 'WeatherForecastController', `graphModel`: 'API_v1', `schemaVersion`: 'v1', `namespace`: 'ModularMonolith.Controllers' };

