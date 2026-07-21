using System.Linq;
using Core.Runtime.Utils;
using UnityEngine;
using VRBuilder.Core.SceneObjects;
using VRBuilder.Core.Utils.Audio;

namespace VRBuilder.Core.User
{
    public class UserService : IUserService
    {
        private IUserConfiguration configuration;
        private IUserSceneObject user;
        private AudioSource instructionPlayer;

        public IUserSceneObject User
        {
            get
            {
                user ??= Object.FindObjectsByType<UserSceneObject>(FindObjectsSortMode.None).FirstOrDefault();
                return user;
            }
            set => user = value;
        }

        public IAudioData InstructionPlayer => instructionPlayer.ToAudioData();

        public AudioSource InstructionAudioSource
        {
            get => instructionPlayer;
            set => instructionPlayer = value;
        }

        public void SetConfiguration(object configuration)
        {
            this.configuration = configuration as IUserConfiguration;
        }

        public void Initialize()
        {
        }
    }
}