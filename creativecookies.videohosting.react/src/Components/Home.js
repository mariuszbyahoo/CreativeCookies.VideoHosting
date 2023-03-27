import { BlobServiceClient } from "@azure/storage-blob";
import { useEffect, useState } from "react";
import styles from "./Home.module.css";

const STORAGE_ACCOUNT_NAME = "mytubestoragecool";
const CONTAINER_NAME = "films";
const DEV_SAS_TOKEN =
  "?sv=2021-12-02&ss=bfqt&srt=c&sp=rwdlacupyx&se=2023-03-27T13:22:02Z&st=2023-03-27T05:22:02Z&sip=79.191.57.150&spr=https,http&sig=F5%2BtAVi8AAwnkEl5ZlUT7ohDLtUPTtppwyxdg9gfyGc%3D";

const Home1 = (props) => {
  const getFilmList = async () => {
    //const sasToken = await fetchContainerSasToken(); po stronie API coś nie bardzo działa, i nie nadaje dostępu do mojego adresu IP albo HTTP / HTTPS
    const sasToken =
      "?sv=2021-12-02&ss=bfqt&srt=c&sp=rwdlacupyx&se=2023-03-27T13:22:02Z&st=2023-03-27T05:22:02Z&sip=79.191.57.150&spr=https,http&sig=F5%2BtAVi8AAwnkEl5ZlUT7ohDLtUPTtppwyxdg9gfyGc%3D";
    const blobServiceClient = new BlobServiceClient(
      `https://${STORAGE_ACCOUNT_NAME}.blob.core.windows.net/?${sasToken}`
    );
    const containerClient =
      blobServiceClient.getContainerClient(CONTAINER_NAME);

    const films = [];
    for await (const blob of containerClient.listBlobsFlat()) {
      films.push(blob);
    }
    console.log("getFilmList func, data: ", films);
    return films;
  };

  const [filmsList, setFilmsList] = useState([]);

  useEffect(() => {
    getFilmList().then((response) => {
      console.log("got response: ", response);
      setFilmsList(response);
    });
  }, [filmsList]);

  const filmList = getFilmList(); // Tutaj robi pełno re-renders
  setFilmsList(filmList);

  const fetchContainerSasToken = async () => {
    const response = await fetch("https://localhost:7276/api/tokens/container");
    const jsonResponse = await response.json();
    return jsonResponse.sasToken;
  };

  const getFilmUrlWithSasToken = async (filmBlobName) => {
    const sasToken = await fetchSasToken(filmBlobName);
    const blobServiceClient = new BlobServiceClient(
      `https://${STORAGE_ACCOUNT_NAME}.blob.core.windows.net/?${sasToken}`
    );
    const containerClient =
      blobServiceClient.getContainerClient(CONTAINER_NAME);
    const blobClient = containerClient.getBlobClient(filmBlobName);

    return blobClient.url;
  };

  const fetchSasToken = async (blobName) => {
    const response = await fetch(
      `https://localhost:7276/api/tokens/blob/${encodeURIComponent(blobName)}`
    );
    const jsonResponse = await response.json();
    return jsonResponse.sasToken;
  };

  return (
    <>
      <div style={{ textAlign: "center" }}>
        <h1>Below the films list:</h1>
        <ul>
          {filmsList.length > 0 &&
            filmsList.map((filmObj) => {
              return <li>Name: {filmObj.name} | URL: NA</li>;
            })}
        </ul>
      </div>
    </>
  );
};

// export default Home;
const fetchContainerSasToken = async () => {
  const response = await fetch("https://localhost:7276/api/tokens/container");
  const jsonResponse = await response.json();
  return jsonResponse.sasToken;
};
const assignContainerSasToken = async (tokenSetter, loadingSetter) => {
  let sasToken = "";
  try {
    sasToken = await fetchContainerSasToken();
    tokenSetter(sasToken);
  } catch (error) {
    console.error("Error fetching container SAS Token:", error);
    loadingSetter(false);
  }
};

const Home = () => {
  const [blobs, setBlobs] = useState([]);
  const [loading, setLoading] = useState(true);
  const [sasToken, setSasToken] = useState("");

  useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      // assignContainerSasToken(setSasToken, setLoading);
      try {
        setLoading(true);
        const blobServiceClient = new BlobServiceClient(
          `https://${STORAGE_ACCOUNT_NAME}.blob.core.windows.net?${DEV_SAS_TOKEN}`
        );

        // BELOW IS NOT WORKING DUE TO INVALID TOKEN FROM API
        // const blobServiceClient = new BlobServiceClient(
        //   `https://${STORAGE_ACCOUNT_NAME}.blob.core.windows.net?${sasToken}`
        // );

        const containerClient =
          blobServiceClient.getContainerClient(CONTAINER_NAME);

        let blobsList = [];
        for await (const blob of containerClient.listBlobsFlat()) {
          blobsList.push(blob.name);
        }

        setBlobs(blobsList);
        setLoading(false);
      } catch (error) {
        console.error("Error fetching blobs from Azure Blob Storage:", error);
        setLoading(false);
      }
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
        {blobs.map((blob, index) => (
          <li key={index}>{blob}</li>
        ))}
      </ul>
    </div>
  );
};

export default Home;
