import {
  FaApple,
  FaDiscord,
  FaFacebook,
  FaGithub,
  FaGoogle,
} from 'react-icons/fa6';

export const SUPPORTED_OAUTH_PROVIDERS = [
  { name: 'github', icon: FaGithub },
  { name: 'discord', icon: FaDiscord },
  { name: 'google', icon: FaGoogle },
  { name: 'facebook', icon: FaFacebook },
  { name: 'apple', icon: FaApple },
] as const;

export type OAuthProvider = (typeof SUPPORTED_OAUTH_PROVIDERS)[number];
export type Provider = OAuthProvider['name'];

export const SUPPORTED_OAUTH_PROVIDERS_MAPPING: Map<Provider, OAuthProvider> =
  new Map(SUPPORTED_OAUTH_PROVIDERS.map(provider => [provider.name, provider]));
