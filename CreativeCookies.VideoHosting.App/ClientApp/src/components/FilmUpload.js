import {
  BlockBlobClient,
  AnonymousCredential,
  newPipeline,
} from "@azure/storage-blob";
import styles from "./FilmUpload.module.css";
import { useState } from "react";
import { Base64 } from "js-base64";

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

  const blockSize = 100 * 1024 * 1024; // 100MB
  const fileSize = file.size;
  const blockCount = Math.ceil(fileSize / blockSize);
  const blockIds = Array.from({ length: blockCount }, (_, i) => {
    const id = ("0000" + i).slice(-5); // Create a 5-character zero-padded string
    const encoder = new TextEncoder();
    const idBytes = encoder.encode(id);
    return Base64.encode(idBytes);
  });

  const blobURL = `https://${account}.blob.core.windows.net/${containerName}/${encodeURIComponent(
    blobName
  )}?${sasToken}`;

  const pipeline = newPipeline(new AnonymousCredential());
  const blobClient = new BlockBlobClient(blobURL, pipeline);

  for (let i = 0; i < blockCount; i++) {
    const start = i * blockSize;
    const end = Math.min(start + blockSize, fileSize);
    const chunk = file.slice(start, end);
    const chunkSize = end - start;
    await blobClient.stageBlock(blockIds[i], chunk, chunkSize);
  }

  await blobClient.commitBlockList(blockIds);
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
