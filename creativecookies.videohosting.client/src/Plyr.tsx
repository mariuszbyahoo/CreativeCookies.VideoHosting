import { useRef } from "react";
import Plyr, { APITypes, PlyrOptions } from "plyr-react";
import "plyr-react/plyr.css";

const videoURL = "https://mytubestoragecool.blob.core.windows.net/films/Konrad%20Berkowicz%2C%20czyli%20podr%C3%B3bka.mp4?sp=r&st=2023-03-15T18:17:38Z&se=2023-03-23T02:17:38Z&spr=https&sv=2021-12-02&sr=b&sig=nOCnKpgBUlQjjh7p41Bkb%2BU6pCowN5HvUyDPBeY0oHo%3D"
//const videoURL = "https://mytubestoragecool.blob.core.windows.net/films/Insurgency%20Sandstorm%202022.04.28%20-%2018.11.21.01.mp4?sv=2021-04-10&st=2023-03-11T18%3A33%3A13Z&se=2023-04-12T17%3A33%3A00Z&sr=b&sp=r&sig=zrmgzo7SKEbGnP1FmSUEuIFHteyesUgsP1OtUdoh3VI%3D";
const provider = "html5";
// const videoOptions: PlyrOptions = {
//   clickToPlay: true,
//   resetOnEnd: true,
//   controls(id, seektime, title) {
    
//   },
//   previewThumbnails: {
//     enabled: true
//   }
// };

const videoOptions = undefined;

const PlyrComponent = () => {
  const ref = useRef<APITypes>(null);

  const enterVideo = () => {
    (ref.current?.plyr as Plyr)?.fullscreen.enter();
  };

  const make2x = () => {
    const plyrInstance = ref.current?.plyr as Plyr;
    if (plyrInstance) plyrInstance.speed = 2;
  };

  const plyrVideo =
    videoURL && provider ? (
      <Plyr
        ref={ref}
        source={{
          type: "video",
          sources: [
            {
              src: videoURL,
              provider: provider
            }
          ]
        }}
        options={videoOptions}
      />
    ) : null;

  return (
    <>
      {plyrVideo}
    </>
  );
};

export default PlyrComponent;
