import { BlobServiceClient } from "@azure/storage-blob";
import { useEffect, useState } from "react";
import styles from "./Home.module.css";
import axios from "axios";

const STORAGE_ACCOUNT_NAME = "mytubestoragecool";
const CONTAINER_NAME = "films";
const DEV_SAS_TOKEN =
  // "sv=2021-12-02&st=2023-03-27T16%3A56%3A06Z&se=2023-03-27T17%3A26%3A06Z&sr=c&sp=l&sig=J2ElY9aG2awsGXlSxPe6VBw12tw%2BVbGNUap3gg32w7U%3D&comp=list&restype=container&_=1679936209254";
  "?sv=2021-12-02&ss=b&srt=c&sp=rwdlacyx&se=2023-03-28T00:03:48Z&st=2023-03-27T16:03:48Z&spr=https,http&sig=F%2F%2B2ZfXng8fSDXs0aDYfqjc74aLUnLhpz9f8KT6v2VI%3D";

// const Home1 = (props) => {
//   const getFilmList = async () => {
//     //const sasToken = await fetchContainerSasToken(); po stronie API coś nie bardzo działa, i nie nadaje dostępu do mojego adresu IP albo HTTP / HTTPS
//     const blobServiceClient = new BlobServiceClient(
//       `https://${STORAGE_ACCOUNT_NAME}.blob.core.windows.net/?${DEV_SAS_TOKEN}`
//     );
//     const containerClient =
//       blobServiceClient.getContainerClient(CONTAINER_NAME);

//     const films = [];
//     for await (const blob of containerClient.listBlobsFlat()) {
//       films.push(blob);
//     }
//     console.log("getFilmList func, data: ", films);
//     return films;
//   };

//   const [filmsList, setFilmsList] = useState([]);

//   useEffect(() => {
//     getFilmList().then((response) => {
//       console.log("got response: ", response);
//       setFilmsList(response);
//     });
//   }, [filmsList]);

//   const filmList = getFilmList(); // Tutaj robi pełno re-renders
//   setFilmsList(filmList);

//   const fetchContainerSasToken = async () => {
//     const response = await fetch("https://localhost:7276/api/tokens/container");
//     const jsonResponse = await response.json();
//     return jsonResponse.sasToken;
//   };

//   const getFilmUrlWithSasToken = async (filmBlobName) => {
//     const sasToken = await fetchSasToken(filmBlobName);
//     const blobServiceClient = new BlobServiceClient(
//       `https://${STORAGE_ACCOUNT_NAME}.blob.core.windows.net/?${sasToken}`
//     );
//     const containerClient =
//       blobServiceClient.getContainerClient(CONTAINER_NAME);
//     const blobClient = containerClient.getBlobClient(filmBlobName);

//     return blobClient.url;
//   };

//   const fetchSasToken = async (blobName) => {
//     const response = await fetch(
//       `https://localhost:7276/api/tokens/blob/${encodeURIComponent(blobName)}`
//     );
//     const jsonResponse = await response.json();
//     return jsonResponse.sasToken;
//   };

//   return (
//     <>
//       <div style={{ textAlign: "center" }}>
//         <h1>Below the films list:</h1>
//         <ul>
//           {filmsList.length > 0 &&
//             filmsList.map((filmObj) => {
//               return <li>Name: {filmObj.name} | URL: NA</li>;
//             })}
//         </ul>
//       </div>
//     </>
//   );
// };
const Home = () => {
  const [blobs, setBlobs] = useState([]);
  const [loading, setLoading] = useState(true);
  // const [sasToken, setSasToken] = useState("");

  useEffect(() => {
    const fetchData = async () => {
      setLoading(true);

      let sasToken = "";

      // const response = await fetch(
      //   "https://localhost:7276/api/tokens/container"
      // )
      //   .then((response) => {
      //     if (!response.ok) {
      //       throw new Error(`HTTP error! status: ${response.status}`);
      //     }
      //     return response.json();
      //   })
      //   .then((data) => {
      //     console.log("received data: ", data);
      //     sasToken = data.sasToken;
      //   })
      //   .catch((error) =>
      //     console.error(
      //       "Error occured when fetching data from the API: ",
      //       error
      //     )
      //   );
      const axiosResponse = axios
        .get("https://localhost:7276/api/tokens/container")
        .then((response) => {
          console.log(`AxiosResponse: ${JSON.stringify(response)}`);
          console.log("response.data.sasToken: ", response.data.sasToken);
          sasToken = response.data.sasToken;
        });

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
