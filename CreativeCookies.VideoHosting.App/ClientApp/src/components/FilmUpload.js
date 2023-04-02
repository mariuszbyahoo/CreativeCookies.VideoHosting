import {
  BlockBlobClient,
  AnonymousCredential,
  newPipeline,
} from "@azure/storage-blob";
import styles from "./FilmUpload.module.css";
import { useState } from "react";

// BELOW SOLUTION IS FOR LARGE FILES, RETURNS 401:

// const uploadFilm = async (file) => {
//   const blobName = file.name;
//   const account = process.env.REACT_APP_STORAGE_ACCOUNT_NAME;
//   const containerName = process.env.REACT_APP_CONTAINER_NAME;
//   const apiAddress = process.env.REACT_APP_API_ADDRESS;

//   const response = await fetch(
//     `https://${apiAddress}/api/SAS/film-upload/${blobName}`
//   );
//   const data = await response.json();
//   //const sasToken = data.sasToken;
//   const sasToken =
//     "?sv=2021-12-02&ss=bfqt&srt=c&sp=rwdlacupyx&se=2023-04-03T01:43:39Z&st=2023-04-02T17:43:39Z&spr=https,http&sig=M7TO7%2BsaJh7UhN9Vi3LOzq2hjwa1EcvD4Bw1yYuSE%2Bw%3D";

//   const blockSize = 100 * 1024 * 1024; // 100MB
//   const fileSize = file.size;
//   const blockCount = Math.ceil(fileSize / blockSize);
//   const blockIds = Array.from({ length: blockCount }, (_, i) => {
//     const id = ("0000" + i).slice(-5); // Create a 5-character zero-padded string
//     const encoder = new TextEncoder();
//     const idBytes = encoder.encode(id);
//     return uint8ArrayToBase64(idBytes);
//   });

//   const blobURL = `https://${account}.blob.core.windows.net/${containerName}/${encodeURIComponent(
//     blobName
//   )}?${sasToken}`;

//   const pipeline = newPipeline(new AnonymousCredential());
//   const blobClient = new BlockBlobClient(blobURL, pipeline);

//   for (let i = 0; i < blockCount; i++) {
//     const start = i * blockSize;
//     const end = Math.min(start + blockSize, fileSize);
//     const chunk = file.slice(start, end);
//     const chunkSize = end - start;
//     await blobClient.stageBlock(blockIds[i], chunk, chunkSize);
//   }

//   await blobClient.commitBlockList(blockIds);
// };

// BELOW WORKS FINE, BUT IS ELIGIBLE ONLY FOR SMALL FILES 
// (uploads it as a single file instead of splitting it for smaller chunks)

const uploadFilm = async (file) => {
  const blobName = file.name;
  const account = process.env.REACT_APP_STORAGE_ACCOUNT_NAME;
  const containerName = process.env.REACT_APP_CONTAINER_NAME;
  const apiAddress = process.env.REACT_APP_API_ADDRESS;

  const response = await fetch(
    `https://${apiAddress}/api/SAS/film-upload/${blobName}`
  );
  const data = await response.json();
  const sasToken = data.sasToken;
  // const sasToken =
  //   "?sv=2021-12-02&ss=bfqt&srt=c&sp=rwdlacupyx&se=2023-04-03T01:43:39Z&st=2023-04-02T17:43:39Z&spr=https,http&sig=M7TO7%2BsaJh7UhN9Vi3LOzq2hjwa1EcvD4Bw1yYuSE%2Bw%3D";

  const blobURL = `https://${account}.blob.core.windows.net/${containerName}/${encodeURIComponent(
    blobName
  )}?${sasToken}`;

  const pipeline = newPipeline(new AnonymousCredential());
  const blobClient = new BlockBlobClient(blobURL, pipeline);

  try {
    const uploadResponse = await blobClient.uploadData(file);
    console.log("Upload response:", uploadResponse);
  } catch (error) {
    console.error("Error uploading file:", error);
  }
};

const uint8ArrayToBase64 = (arr) => {
  const base64url =
    "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
  let base64string = "";
  let padding = 0;

  for (let i = 0; i < arr.length; i += 3) {
    const a = arr[i];
    const b = arr[i + 1];
    const c = arr[i + 2];
    const triplet = (a << 16) | (b << 8) | c;

    base64string += base64url.charAt((triplet >>> 18) & 63);
    base64string += base64url.charAt((triplet >>> 12) & 63);
    base64string +=
      b === undefined ? "" : base64url.charAt((triplet >>> 6) & 63);
    base64string += c === undefined ? "" : base64url.charAt(triplet & 63);

    if (c === undefined) {
      padding++;
    }
    if (b === undefined) {
      padding++;
    }
  }

  return (
    base64string.slice(0, base64string.length - padding) + "=".repeat(padding)
  );
};

const FilmUpload = (props) => {
  const [file, setFile] = useState();

  const description = `https://${process.env.REACT_APP_STORAGE_ACCOUNT_NAME}.blob.core.windows.net/${process.env.REACT_APP_CONTAINER_NAME}/`;

  const fileChangeHandler = (e) => {
    if (e.target.files) {
      setFile(e.target.files[0]);
    }
  };

  const fileUploadHandler = () => {
    if (!file) {
      return;
    }

    uploadFilm(file);
  };

  return (
    <div className={styles.container}>
      Upload a file below to add it into films container into{" "}
      {process.env.REACT_APP_STORAGE_ACCOUNT_NAME}
      <br />
      <input
        type="file"
        placeholder="Select film to upload"
        onChange={fileChangeHandler}
      />
      <br />
      <button type="submit" onClick={fileUploadHandler}>
        Upload
      </button>
      <br />
      <br />
      {file && <p>Title of the uploaded film: {file.name}</p>}
    </div>
  );
};

export default FilmUpload;
