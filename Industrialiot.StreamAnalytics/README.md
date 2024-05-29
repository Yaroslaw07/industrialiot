# Stream Analytics

This documentation outlines the configuration and usage of an Azure Stream Analytics job, detailing the query logic and providing examples of the resulting data stored in Azure Blob Storage.

## Prerequisites

Ensure you have:

- Configured input sources (e.g., IoT Hub, Event Hub)
- Configured output to Azure Blob Storage
- Copy transformation to Stream Analytic Job query

## Data analysis

### Production Rate

- Query that calculates the production rate metrics for IoT devices, including the number of good counts, total volume, and the percentage of good production over a 5-minute tumbling window.

  ```sql
  SELECT
      IoTHub.ConnectionDeviceId,
      System.Timestamp AS CurrentTime,
      SUM(GoodCount) AS GoodCount,
      SUM(GoodCount + BadCount) AS TotalVolume,
      1.0 * SUM(GoodCount) / SUM(GoodCount + BadCount) AS GoodProductionPercentage
  INTO
      [kpi-store]
  FROM
      [iothub] TIMESTAMP BY EventEnqueuedUtcTime
  WHERE
      GetMetadataPropertyValue([iothub], '[User].[type]') = 'Telemetry'
  GROUP BY
      TumblingWindow(minute, 5), IoTHub.ConnectionDeviceId;
  ```

  ***

  Result in blob (json)

  ```json
  {"ConnectionDeviceId":"Device1","CurrentTime":"2024-05-17T22:05:00.0000000Z","GoodCount":780.0,"TotalVolume":901.0,"GoodProductionPercentage":0.8657047724750278}
  {"ConnectionDeviceId":"Device2","CurrentTime":"2024-05-17T22:05:00.0000000Z","GoodCount":1159.0,"TotalVolume":1301.0,"GoodProductionPercentage":0.8908531898539584}
  ```

- This query filters out production metrics where the good production percentage is below 90%.
  ```sql
  SELECT
      ConnectionDeviceId,
      CurrentTime,
      GoodCount,
      TotalVolume,
      GoodProductionPercentage
  INTO
      [lower-production-kpi]
  FROM
      [production-kpi]
  WHERE
      GoodProductionPercentage != 0 AND GoodProductionPercentage < 0.90;
  ```

### Temperature

- This query calculates the average, minimum, and maximum temperatures over a 5-minute hopping window, advancing every minute.

```sql
SELECT
    IoTHub.ConnectionDeviceId,
    System.Timestamp AS CurrentTime,
    AVG(Temperature) AS AvgTemperature,
    MIN(Temperature) AS MinTemperature,
    MAX(Temperature) AS MaxTemperature
INTO
    [temperature-store]
FROM
    [iothub] TIMESTAMP BY EventEnqueuedUtcTime
WHERE
    GetMetadataPropertyValue([iothub], '[User].[type]') = 'Telemetry'
GROUP BY
    HoppingWindow(minute, 5, 1), IoTHub.ConnectionDeviceId;
```

---

Result in blob (json)

```json
{"ConnectionDeviceId":"Device1","CurrentTime":"2024-05-16T19:20:00.0000000Z","AvgTemperature":5.9408719178030704,"MinTemperature":-383.0,"MaxTemperature":25.904412497765282}
{"ConnectionDeviceId":"Device2","CurrentTime":"2024-05-16T19:20:00.0000000Z","AvgTemperature":25.05542061524205,"MinTemperature":24.182312212884465,"MaxTemperature":25.844266760807855}
```

### Errors

- This query captures new device errors to event hub when the error count is greater than zero.

```sql
    SELECT
    IoTHub.ConnectionDeviceId,
    deviceError,
    System.Timestamp AS CurrentTime
INTO
    [new-error]
FROM
    [iothub] TIMESTAMP BY EventEnqueuedUtcTime
WHERE
    GetMetadataPropertyValue([iothub], '[User].[type]') = 'DeviceError' AND newErrorsCount > 0
```

- This query identifies devices with more than three errors in the past minute using a sliding window and send to blob.

```sql
SELECT
    IoTHub.ConnectionDeviceId,
    System.Timestamp AS CurrentTime
INTO
    [lot-errors]
FROM
    [iothub] TIMESTAMP BY EventEnqueuedUtcTime
WHERE
    GetMetadataPropertyValue([iothub], '[User].[type]') = 'DeviceError'
GROUP BY
    SlidingWindow(minute, 1), IoTHub.ConnectionDeviceId
HAVING
    COUNT(newErrorsCount) > 3;
```

---

Result in blob(json)

```json
{
  "ConnectionDeviceId": "Device1",
  "CurrentTime": "2024-05-16T19:24:24.4210000Z"
}
```

- This query handles emergency stop events based on lot-errors-input event hub.

```sql
SELECT
    IoTHub.ConnectionDeviceId,
    System.Timestamp AS CurrentTime
INTO
    [emergency-stop]
FROM
    [lot-errors-input] TIMESTAMP BY EventEnqueuedUtcTime;
```
