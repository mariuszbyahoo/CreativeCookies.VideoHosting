import { BlobServiceClient } from "@azure/storage-blob";
import { useEffect, useState } from "react";
import styles from "./Home.module.css";
import axios from "axios";

const STORAGE_ACCOUNT_NAME = "mytubestoragecool";
const CONTAINER_NAME = "films";
const DEV_SAS_TOKEN =
  // "sv=2021-12-02&st=2023-03-27T16%3A56%3A06Z&se=2023-03-27T17%3A26%3A06Z&sr=c&sp=l&sig=J2ElY9aG2awsGXlSxPe6VBw12tw%2BVbGNUap3gg32w7U%3D&comp=list&restype=container&_=1679936209254";
  "?sv=2021-12-02&ss=b&srt=c&sp=rwdlacyx&se=2023-03-28T00:03:48Z&st=2023-03-27T16:03:48Z&spr=https,http&sig=F%2F%2B2ZfXng8fSDXs0aDYfqjc74aLUnLhpz9f8KT6v2VI%3D";

// REACT CODE SEEMS NOT OK, NOW API RETURNS VALID TOKENS

async function fetchSasToken() {
  const response = await fetch(`https://localhost:7276/api/Tokens/container`);
  const data = await response.json();
  return data.sasToken;
}

async function listBlobs(containerName, sasToken) {
  const blobServiceClient = new BlobServiceClient(
    `https://${STORAGE_ACCOUNT_NAME}.blob.core.windows.net/?${sasToken}`
  );

  const containerClient = blobServiceClient.getContainerClient(containerName);
  const blobs = [];
  for await (const blob of containerClient.listBlobsFlat()) {
    blobs.push(blob.name);
  }

  return blobs;
}

const Home = () => {
  const [blobs, setBlobs] = useState([]);
  const [loading, setLoading] = useState(true);
  const [sasToken, setSasToken] = useState("");
  const [xmlResponse, setXmlResponse] = useState("");
  useEffect(() => {
    setLoading(true);
    setSasToken(fetchSasToken());
    console.log("fetchSasToken with fetch API: ", fetchSasToken());

    const fetchData = async () => {
      let sasToken = "";
      // const axiosResponse = axios
      //   .get("https://localhost:7276/api/tokens/container")
      //   .then((response) => {
      //     console.log(`AxiosResponse: ${JSON.stringify(response)}`);
      //     console.log("response.data.sasToken: ", response.data.sasToken);
      //     setSasToken(response.data.sasToken);
      //   });

      let data = "";
      fetch(
        `https://${STORAGE_ACCOUNT_NAME}.blob.core.windows.net/films?restype=container&comp=list&${sasToken}`
      ).then((response) => {
        setXmlResponse(response);
      });

      setBlobs(await listBlobs("films", sasToken));
    };

    fetchData();
  }, []); // Empty array to run the effect only once, when the component mounts.

  if (loading) {
    return <div>Loading...</div>;
  }

  return (
    <div className={styles.container}>
      <h1>Blobs in the Azure Blob Storage Container:</h1>
      <ul>
        <h4>XML Response:</h4>
        <div>{{ xmlResponse }}</div>
        {blobs.map((blob, index) => (
          <li key={index}>{blob}</li>
        ))}
      </ul>
    </div>
  );
};

export default Home;
