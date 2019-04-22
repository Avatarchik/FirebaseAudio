//-----------------------------------------------------------------------
// <copyright file="AugmentedImageVisualizer.cs" company="Google">
//
// Copyright 2018 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleARCore.Examples.AugmentedImage
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using GoogleARCore;
    using GoogleARCoreInternal;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Uses 4 frame corner objects to visualize an AugmentedImage.
    /// </summary>
    public class AugmentedImageVisualizer : MonoBehaviour
    {
        /// <summary>
        /// The AugmentedImage to visualize.
        /// </summary>
        public AugmentedImage Image;

        /// <summary>
        /// A model for the lower left corner of the frame to place when an image is detected.
        /// </summary>
        public GameObject Sphere;

        public AudioClip MusicClip;
        public AudioSource MusicSource;
        public float[] position = new float[] { 0, 0, 0 };
        public bool first = false;
        public List<GameObject> spheresList;
        private Anchor anchor;
        public List<float[]> positionsList;
        public Text text;



        /// <summary>
        /// The Unity Update method.
        /// </summary>

        public void Start()
        {
            anchor = Image.CreateAnchor(Image.CenterPose);
            float x = 0.2f;
            float y = 0.2f;
            float z = 0.2f;
            //Destroy(Sphere);
            for (int i = 0; i < 3; i++) {
                /*
                GameObject s = (GameObject)Instantiate(Sphere, anchor.transform);
                s.transform.localPosition =
                          (positionsList[i][0] * Vector3.right) + (positionsList[i][1] * Vector3.forward) +
                          (positionsList[i][2] * Vector3.down);
                s.SetActive(true);
                */

                GameObject s2 = (GameObject)Instantiate(Sphere, anchor.transform);
                s2.transform.localPosition =
                          (x * Vector3.right) + (y * Vector3.forward) +
                          (z * Vector3.down);
                x += 0.1f;
                y += 0.1f;
                z += 0.1f;
                s2.SetActive(true);
                spheresList.Add(s2);

            }
            text.text += "hehe ";

        }


        public void Update()
        {
            if (Image == null || Image.TrackingState != TrackingState.Tracking)
            {
                //Sphere.SetActive(false);
                return;
            }
            int sCount = spheresList.Count;
            int pCount = positionsList.Count;

            if (sCount - 3 < pCount) 
            {
                text.text = "true";

                for (int i = sCount - 3; i < pCount; i++) 
                {
                    GameObject s = (GameObject)Instantiate(Sphere, anchor.transform);
                    s.transform.localPosition =
                              (positionsList[i][0] * Vector3.right) + (positionsList[i][1] * Vector3.forward) +
                              (positionsList[i][2] * Vector3.down);
                    spheresList.Add(s);
                    s.SetActive(true);
                }

            }

            Sphere.transform.localPosition =
                      (1 * Vector3.left) + (1 * Vector3.forward) +
                      (1 * Vector3.down);

            Sphere.SetActive(false);

            text.text = positionsList[0][0] + "" + positionsList[0][1] + "" + positionsList[0][2] + "" + positionsList.Count + ", " + spheresList.Count;
           

        }

        public void Destroy()
        {
            foreach (GameObject s in spheresList) {
                Destroy(s);
            }
            //spheresList.RemoveRange(0, spheresList.Count);
        }

        public void Play()
        {
            //MusicSource.clip = MusicClip;
            //MusicSource.Play();
        }
    }
}
