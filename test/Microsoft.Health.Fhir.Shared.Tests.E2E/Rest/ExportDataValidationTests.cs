﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Health.Fhir.Core.Features.Operations.Export.Models;
using Microsoft.Health.Fhir.Core.Models;
using Microsoft.Health.Fhir.Tests.Common;
using Microsoft.Health.Fhir.Tests.Common.FixtureParameters;
using Microsoft.Health.Fhir.Tests.E2E.Common;
using Microsoft.Health.Test.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;
using FhirGroup = Hl7.Fhir.Model.Group;
using Task = System.Threading.Tasks.Task;

namespace Microsoft.Health.Fhir.Tests.E2E.Rest
{
    [Trait(Traits.Category, Categories.ExportDataValidation)]
    [HttpIntegrationFixtureArgumentSets(DataStore.All, Format.Json)]
    public class ExportDataValidationTests : IClassFixture<HttpIntegrationTestFixture>
    {
        private readonly TestFhirClient _testFhirClient;
        private readonly ITestOutputHelper _outputHelper;

        public ExportDataValidationTests(HttpIntegrationTestFixture fixture, ITestOutputHelper testOutputHelper)
        {
            _testFhirClient = fixture.TestFhirClient;
            _outputHelper = testOutputHelper;
        }

        /* Disabled due to Bug 75001
        [Fact]
        public async Task GivenFhirServer_WhenAllDataIsExported_ThenExportedDataIsSameAsDataInFhirServer()
        {
            // NOTE: Azure Storage Emulator is required to run these tests locally.

            // Trigger export request and check for export status
            Uri contentLocation = await _testFhirClient.ExportAsync();
            IList<Uri> blobUris = await CheckExportStatus(contentLocation);

            // Download exported data from storage account
            Dictionary<(string resourceType, string resourceId), string> dataFromExport = await DownloadBlobAndParse(blobUris);

            // Download all resources from fhir server
            Dictionary<(string resourceType, string resourceId), string> dataFromFhirServer = await GetResourcesFromFhirServer(_testFhirClient.HttpClient.BaseAddress);

            // Assert both data are equal
            Assert.True(ValidateDataFromBothSources(dataFromFhirServer, dataFromExport));
        }

        [Fact]
        public async Task GivenFhirServer_WhenPatientDataIsExported_ThenExportedDataIsSameAsDataInFhirServer()
        {
            // NOTE: Azure Storage Emulator is required to run these tests locally.

            // Trigger export request and check for export status
            Uri contentLocation = await _testFhirClient.ExportAsync("Patient/");
            IList<Uri> blobUris = await CheckExportStatus(contentLocation);

            // Download exported data from storage account
            Dictionary<(string resourceType, string resourceId), string> dataFromExport = await DownloadBlobAndParse(blobUris);

            // Download resources from fhir server
            Uri address = new Uri(_testFhirClient.HttpClient.BaseAddress, "Patient/");
            Dictionary<(string resourceType, string resourceId), string> dataFromFhirServer = await GetResourcesFromFhirServer(address);

            Dictionary<(string resourceType, string resourceId), string> compartmentData = new Dictionary<(string resourceType, string resourceId), string>();
            foreach ((string resourceType, string resourceId) key in dataFromFhirServer.Keys)
            {
                address = new Uri(_testFhirClient.HttpClient.BaseAddress, "Patient/" + key.resourceId + "/*");

                // copies all the new values into the compartment data dictionary
                (await GetResourcesFromFhirServer(address)).ToList().ForEach(x => compartmentData.TryAdd(x.Key, x.Value));
            }

            compartmentData.ToList().ForEach(x => dataFromFhirServer.TryAdd(x.Key, x.Value));
            dataFromFhirServer.Union(compartmentData);

            // Assert both data are equal
            Assert.True(ValidateDataFromBothSources(dataFromFhirServer, dataFromExport));
        }

        [Fact]
        public async Task GivenFhirServer_WhenAllObservationAndPatientDataIsExported_ThenExportedDataIsSameAsDataInFhirServer()
        {
            // NOTE: Azure Storage Emulator is required to run these tests locally.

            // Trigger export request and check for export status
            Uri contentLocation = await _testFhirClient.ExportAsync(string.Empty, "_type=Observation,Patient");
            IList<Uri> blobUris = await CheckExportStatus(contentLocation);

            // Download exported data from storage account
            Dictionary<(string resourceType, string resourceId), string> dataFromExport = await DownloadBlobAndParse(blobUris);

            // Download resources from fhir server
            Uri address = new Uri(_testFhirClient.HttpClient.BaseAddress, "?_type=Observation,Patient");
            Dictionary<(string resourceType, string resourceId), string> dataFromFhirServer = await GetResourcesFromFhirServer(address);

            // Assert both data are equal
            Assert.True(ValidateDataFromBothSources(dataFromFhirServer, dataFromExport));
        }

        [Fact]
        public async Task GivenFhirServer_WhenPatientObservationDataIsExported_ThenExportedDataIsSameAsDataInFhirServer()
        {
            // NOTE: Azure Storage Emulator is required to run these tests locally.

            // Trigger export request and check for export status
            Uri contentLocation = await _testFhirClient.ExportAsync("Patient/", "_type=Observation");
            IList<Uri> blobUris = await CheckExportStatus(contentLocation);

            // Download exported data from storage account
            Dictionary<(string resourceType, string resourceId), string> dataFromExport = await DownloadBlobAndParse(blobUris);

            // Download resources from fhir server
            Uri address = new Uri(_testFhirClient.HttpClient.BaseAddress, "Patient/");
            Dictionary<(string resourceType, string resourceId), string> dataFromFhirServer = await GetResourcesFromFhirServer(address);

            Dictionary<(string resourceType, string resourceId), string> compartmentData = new Dictionary<(string resourceType, string resourceId), string>();
            foreach ((string resourceType, string resourceId) key in dataFromFhirServer.Keys)
            {
                address = new Uri(_testFhirClient.HttpClient.BaseAddress, "Patient/" + key.resourceId + "/Observation");

                // copies all the new values into the compartment data dictionary
                (await GetResourcesFromFhirServer(address)).ToList().ForEach(x => compartmentData.TryAdd(x.Key, x.Value));
            }

            compartmentData.ToList().ForEach(x => dataFromFhirServer.TryAdd(x.Key, x.Value));
            dataFromFhirServer.Union(compartmentData);

            // Assert both data are equal
            Assert.True(ValidateDataFromBothSources(dataFromFhirServer, dataFromExport));
        }
        */

        [Fact]
        public async Task GivenFhirServer_WhenGroupDataIsExported_ThenExportedDataIsSameAsDataInFhirServer()
        {
            // NOTE: Azure Storage Emulator is required to run these tests locally.

            // Add data for test
            var (dataInFhirServer, groupId) = await CreateGroupWithPatient(true);

            // Trigger export request and check for export status
            Uri contentLocation = await _testFhirClient.ExportAsync($"Group/{groupId}/");
            IList<Uri> blobUris = await CheckExportStatus(contentLocation);

            // Download exported data from storage account
            Dictionary<(string resourceType, string resourceId), string> dataFromExport = await DownloadBlobAndParse(blobUris);

            // Assert both data are equal
            // Also remember to add support for the inactive flag in Group member (probably best in group member extractor as a flag on the Get method)
            Assert.True(ValidateDataFromBothSources(dataInFhirServer, dataFromExport));
        }

        [Fact]
        public async Task GivenFhirServer_WhenGroupObervationDataIsExported_ThenExportedDataIsSameAsDataInFhirServer()
        {
            // NOTE: Azure Storage Emulator is required to run these tests locally.

            // Add data for test
            var (dataInFhirServer, groupId) = await CreateGroupWithPatient(false);

            // Trigger export request and check for export status
            Uri contentLocation = await _testFhirClient.ExportAsync($"Group/{groupId}/", "_type=Encounter");
            IList<Uri> blobUris = await CheckExportStatus(contentLocation);

            // Download exported data from storage account
            Dictionary<(string resourceType, string resourceId), string> dataFromExport = await DownloadBlobAndParse(blobUris);

            // Assert both data are equal
            Assert.True(ValidateDataFromBothSources(dataInFhirServer, dataFromExport));
        }

        private bool ValidateDataFromBothSources(Dictionary<(string resourceType, string resourceId), string> dataFromServer, Dictionary<(string resourceType, string resourceId), string> dataFromStorageAccount)
        {
            bool result = true;

            if (dataFromStorageAccount.Count != dataFromServer.Count)
            {
                _outputHelper.WriteLine($"Count differs. Exported data count: {dataFromStorageAccount.Count} Fhir Server Count: {dataFromServer.Count}");
                result = false;

                foreach (KeyValuePair<(string resourceType, string resourceId), string> kvp in dataFromStorageAccount)
                {
                    if (!dataFromServer.ContainsKey(kvp.Key))
                    {
                        _outputHelper.WriteLine($"Extra resource in exported data: {kvp.Key}");
                    }
                }
            }

            // Enable this check when creating/updating data validation tests to ensure there is data to export
            /*
            if (dataFromStorageAccount.Count == 0)
            {
                _outputHelper.WriteLine("No data exported. This test expects data to be present.");
                return false;
            }
            */

            int wrongCount = 0;
            foreach (KeyValuePair<(string resourceType, string resourceId), string> kvp in dataFromServer)
            {
                if (!dataFromStorageAccount.ContainsKey(kvp.Key))
                {
                    _outputHelper.WriteLine($"Missing resource from exported data: {kvp.Key}");
                    result = false;
                    wrongCount++;
                    continue;
                }

                string exportEntry = dataFromStorageAccount[kvp.Key];
                string serverEntry = kvp.Value;
                if (!serverEntry.Equals(exportEntry))
                {
                    _outputHelper.WriteLine($"Exported resource does not match server resource: {kvp.Key}");
                    result = false;
                    wrongCount++;
                    continue;
                }
            }

            _outputHelper.WriteLine($"Missing or wrong match count: {wrongCount}");
            return result;
        }

        // Check export status and return output once we get 200
        private async Task<IList<Uri>> CheckExportStatus(Uri contentLocation)
        {
            HttpStatusCode resultCode = HttpStatusCode.Accepted;
            HttpResponseMessage response = null;
            while (resultCode == HttpStatusCode.Accepted)
            {
                await Task.Delay(5000);

                response = await _testFhirClient.CheckExportAsync(contentLocation);

                resultCode = response.StatusCode;
            }

            if (resultCode != HttpStatusCode.OK)
            {
                throw new Exception($"Export request failed with status code {resultCode}");
            }

            // we have got the result. Deserialize into output response.
            var contentString = await response.Content.ReadAsStringAsync();

            ExportJobResult exportJobResult = JsonConvert.DeserializeObject<ExportJobResult>(contentString);
            return exportJobResult.Output.Select(x => x.FileUri).ToList();
        }

        private async Task<Dictionary<(string resourceType, string resourceId), string>> DownloadBlobAndParse(IList<Uri> blobUri)
        {
            if (blobUri == null || blobUri.Count == 0)
            {
                return new Dictionary<(string resourceType, string resourceId), string>();
            }

            // Extract storage account name from blob uri in order to get corresponding access token.
            Uri sampleUri = blobUri[0];
            string storageAccountName = sampleUri.Host.Split('.')[0];

            CloudStorageAccount cloudAccount = GetCloudStorageAccountHelper(storageAccountName);
            CloudBlobClient blobClient = cloudAccount.CreateCloudBlobClient();
            var resourceIdToResourceMapping = new Dictionary<(string resourceType, string resourceId), string>();

            foreach (Uri uri in blobUri)
            {
                var blob = new CloudBlockBlob(uri, blobClient);
                string allData = await blob.DownloadTextAsync();

                var splitData = allData.Split("\n");

                foreach (var entry in splitData)
                {
                    if (string.IsNullOrWhiteSpace(entry))
                    {
                        continue;
                    }

                    JObject resource = JObject.Parse(entry);

                    // Ideally this should just be Add, but until we prevent duplicates from being added to the server there is a chance the same resource being added multiple times resulting in a key conflict.
                    resourceIdToResourceMapping.TryAdd((resource["resourceType"].ToString(), resource["id"].ToString()), entry);
                }
            }

            // Delete the container since we have downloaded the data.
            Regex regex = new Regex(@"(?<guid>[a-f0-9]{8}(?:\-[a-f0-9]{4}){3}\-[a-f0-9]{12})");
            var guidMatch = regex.Match(blobUri[0].ToString());
            CloudBlobContainer container = blobClient.GetContainerReference(guidMatch.Value);
            await container.DeleteIfExistsAsync();

            return resourceIdToResourceMapping;
        }

        private async Task<Dictionary<(string resourceType, string resourceId), string>> GetResourcesFromFhirServer(Uri requestUri)
        {
            var resourceIdToResourceMapping = new Dictionary<(string resourceType, string resourceId), string>();

            while (requestUri != null)
            {
                HttpRequestMessage request = new HttpRequestMessage()
                {
                    RequestUri = requestUri,
                    Method = HttpMethod.Get,
                };

                using HttpResponseMessage response = await _testFhirClient.HttpClient.SendAsync(request);

                var responseString = await response.Content.ReadAsStringAsync();

                JObject result = JObject.Parse(responseString);

                JArray entries = (JArray)result["entry"];

                // Fhir server has not returned any data. Return existing mapping.
                if (entries == null)
                {
                    return resourceIdToResourceMapping;
                }

                foreach (JToken entry in entries)
                {
                    string resourceType = entry["resource"]["resourceType"].ToString();
                    string id = entry["resource"]["id"].ToString();

                    string resource = entry["resource"].ToString().Trim();

                    resourceIdToResourceMapping.TryAdd((resourceType, id), resource);
                }

                // Look at whether a continuation token has been returned.
                // We will always have self link. We are looking for the "next" link
                JArray links = (JArray)result["link"];
                string nextUri = null;
                if (links != null && links.Count > 1)
                {
                    foreach (JToken link in links)
                    {
                        if (link["relation"].ToString() == "next")
                        {
                            nextUri = link["url"].ToString();
                            break;
                        }
                    }
                }

                requestUri = nextUri == null ? null : new Uri(nextUri);
            }

            return resourceIdToResourceMapping;
        }

        private CloudStorageAccount GetCloudStorageAccountHelper(string storageAccountName)
        {
            if (string.IsNullOrWhiteSpace(storageAccountName))
            {
                throw new Exception("StorageAccountName cannot be empty");
            }

            CloudStorageAccount cloudAccount = null;

            // If we are running locally, then we need to connect to the Azure Storage Emulator.
            // Else we need to connect to a proper Azure Storage Account.
            if (storageAccountName.Equals("127", StringComparison.OrdinalIgnoreCase))
            {
                string emulatorConnectionString = "UseDevelopmentStorage=true";
                CloudStorageAccount.TryParse(emulatorConnectionString, out cloudAccount);
            }
            else
            {
                string storageSecret = Environment.GetEnvironmentVariable(storageAccountName + "_secret");
                if (!string.IsNullOrWhiteSpace(storageSecret))
                {
                    StorageCredentials storageCredentials = new StorageCredentials(storageAccountName, storageSecret);
                    cloudAccount = new CloudStorageAccount(storageCredentials, useHttps: true);
                }
            }

            if (cloudAccount == null)
            {
                throw new Exception("Unable to create a cloud storage account");
            }

            return cloudAccount;
        }

        private async Task<(Dictionary<(string resourceType, string resourceId), string> serverData, string groupId)> CreateGroupWithPatient(bool includeObservationInServerData)
        {
            // Add data for test
            var patient = new Patient();
            var patientResponse = await _testFhirClient.CreateAsync(patient);
            var patientId = patientResponse.Resource.Id;

            var encounter = new Encounter()
            {
                Status = Encounter.EncounterStatus.InProgress,
                Class = new Coding()
                {
                    Code = "test",
                },
                Subject = new ResourceReference($"{KnownResourceTypes.Patient}/{patientId}"),
            };

            var encounterResponse = await _testFhirClient.CreateAsync(encounter);
            var encounterId = encounterResponse.Resource.Id;

            var observation = new Observation()
            {
                Status = ObservationStatus.Final,
                Code = new CodeableConcept()
                {
                    Coding = new List<Coding>()
                    {
                        new Coding()
                        {
                            Code = "test",
                        },
                    },
                },
                Subject = new ResourceReference($"{KnownResourceTypes.Patient}/{patientId}"),
            };

            var observationResponse = await _testFhirClient.CreateAsync(observation);
            var observationId = observationResponse.Resource.Id;

            var group = new FhirGroup()
            {
                Type = FhirGroup.GroupType.Person,
                Actual = true,
                Member = new List<FhirGroup.MemberComponent>()
                {
                    new FhirGroup.MemberComponent()
                    {
                        Entity = new ResourceReference($"{KnownResourceTypes.Patient}/{patientId}"),
                    },
                },
            };

            var groupResponse = await _testFhirClient.CreateAsync(group);
            var groupId = groupResponse.Resource.Id;

            var resourceDictionary = new Dictionary<(string resourceType, string resourceId), string>();
            resourceDictionary.Add((KnownResourceTypes.Patient, patientId), patientResponse.Resource.ToJson().Trim());
            resourceDictionary.Add((KnownResourceTypes.Encounter, encounterId), encounterResponse.Resource.ToJson().Trim());

            if (includeObservationInServerData)
            {
                resourceDictionary.Add((KnownResourceTypes.Observation, observationId), observationResponse.Resource.ToJson().Trim());
            }

            return (resourceDictionary, groupId);
        }
    }
}
