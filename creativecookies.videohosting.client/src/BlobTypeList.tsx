import * as React from 'react';
import { BlobServiceClient, BlobType } from '@azure/storage-blob';

interface BlobTypeItem {
  name: string;
  type: string;
}

interface BlobTypeListProps {
  connectionString: string;
  containerName: string;
}

const BlobTypeListComponent: React.FC<BlobTypeListProps> = ({ connectionString, containerName }) => {
  const [blobTypes, setBlobTypes] = React.useState<BlobTypeItem[]>([]);
  const [loading, setLoading] = React.useState<boolean>(true);
  const [error, setError] = React.useState<string | null>(null);

  React.useEffect(() => {
    const getBlobTypes = async () => {
      try {
        const blobServiceClient = new BlobServiceClient(connectionString);
        const containerClient = blobServiceClient.getContainerClient(containerName);
        const blobs = containerClient.listBlobsFlat({ includeMetadata: true });

        const newBlobTypes: BlobTypeItem[] = [];

        for await (const blob of blobs) {
          const blobType = blob.properties.blobType;
          const contentType = blob.properties.contentType;

          if (blobType === "BlockBlob" && contentType === 'video/mp4') {
            newBlobTypes.push({
              name: blob.name,
              type: contentType,
            });
          }
        }

        setBlobTypes(newBlobTypes);
        setLoading(false);
      } catch (error: any) {
        setLoading(false);
        setError(error.message);
      }
    };

    getBlobTypes();
  }, [connectionString, containerName]);

  if (loading) {
    return <p>Loading...</p>;
  }

  if (error) {
    return <p>Error: {error}</p>;
  }

  return (
    <div>
      <h2>Available MP4 Blob Types:</h2>
      <ul>
        {blobTypes.map((blobType, index) => (
          <li key={index}>
            <strong>{blobType.name}</strong> ({blobType.type})
          </li>
        ))}
      </ul>
    </div>
  );
};

export default BlobTypeListComponent;
