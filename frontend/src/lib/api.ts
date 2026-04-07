export class ApiError extends Error {
  public readonly status: number;

  constructor(message: string, status: number) {
    super(message);
    this.name = "ApiError";
    this.status = status;
  }
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  hasMore: boolean;
}

export interface Sport {
  id: number;
  name: string;
  slug: string;
  description: string;
  isOlympic: boolean;
}

export interface Competition {
  id: number;
  sportId: number;
  sport: string;
  name: string;
  country: string;
  format: string;
  isCup: boolean;
}

export interface Season {
  id: number;
  competitionId: number;
  name: string;
  yearStart: number;
  yearEnd: number;
  isCurrent: boolean;
}

export interface Match {
  id: number;
  competitionId: number;
  seasonId: number;
  sportId: number;
  competition: string;
  homeTeam: string;
  awayTeam: string;
  kickoffUtc: string;
  status: string;
  homeScore: number;
  awayScore: number;
  venue: string;
}

export interface MatchEvent {
  minute: number;
  type: string;
  subject: string;
}

export interface MatchDetail {
  match: Match;
  events: MatchEvent[];
}

export interface Team {
  id: number;
  sportId: number;
  name: string;
  shortName: string;
  country: string;
  venue: string;
  founded: number;
  badgeUrl: string;
}

export interface Person {
  id: number;
  teamId: number;
  fullName: string;
  role: string;
  nationality: string;
  birthDate: string;
  shirtNumber: number | null;
  photoUrl: string;
}

export interface PersonProfile {
  person: Person;
  bio: string;
  teamName: string;
}

export interface TeamRoster {
  team: Team;
  members: Person[];
}

export interface StandingRow {
  position: number;
  team: string;
  played: number;
  won: number;
  drawn: number;
  lost: number;
  goalsFor: number;
  goalsAgainst: number;
  points: number;
}

export interface SeasonDetail {
  competition: Competition;
  season: Season;
  standings: StandingRow[];
}

export interface PlayerRanking {
  position: number;
  person: string;
  team: string;
  category: string;
  value: number;
}

export interface AdminIdentity {
  userName: string;
  displayName: string;
  role: string;
}

export interface AdminLoginResponse {
  token: string;
  expiresAtUtc: string;
  user: AdminIdentity;
}

export interface AdminUserUpsertRequest {
  userName: string;
  displayName: string;
  role: "superadmin" | "editor" | "readonly";
  password: string;
  isActive: boolean;
}

export interface ChangeLogEntry {
  id: number;
  action: string;
  entityName: string;
  adminUserName: string;
  timestamp: string;
  summary: string;
}

const API_BASE_URL = import.meta.env.VITE_API_URL ?? "";

async function request<T>(path: string, init: RequestInit = {}): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...(init.headers ?? {}),
    },
  });

  if (!response.ok) {
    let message = `Request failed with status ${response.status}`;

    try {
      const errorBody = (await response.json()) as {
        detail?: string;
        title?: string;
      };
      message = errorBody.detail ?? errorBody.title ?? message;
    } catch {
      // Ignore JSON parsing errors for non-JSON responses.
    }

    throw new ApiError(message, response.status);
  }

  return (await response.json()) as T;
}

export const api = {
  getSports: () =>
    request<PagedResult<Sport>>("/api/sports?page=1&pageSize=12"),
  getCompetitions: () =>
    request<PagedResult<Competition>>("/api/competitions?page=1&pageSize=12"),
  getCompetitionSeasons: (competitionId: number) =>
    request<PagedResult<Season>>(
      `/api/competitions/${competitionId}/seasons?page=1&pageSize=10`,
    ),
  getSeasonDetail: (seasonId: number) =>
    request<SeasonDetail>(`/api/competitions/seasons/${seasonId}`),
  getLiveMatches: () => request<Match[]>("/api/matches/live"),
  getUpcomingMatches: () => request<Match[]>("/api/matches/upcoming?count=8"),
  getMatchDetail: (matchId: number) =>
    request<MatchDetail>(`/api/matches/${matchId}`),
  getTeams: () => request<PagedResult<Team>>("/api/teams?page=1&pageSize=12"),
  getTeamRoster: (teamId: number) =>
    request<TeamRoster>(`/api/teams/${teamId}/roster`),
  searchPeople: (query = "") =>
    request<PagedResult<Person>>(
      `/api/people/search?query=${encodeURIComponent(query)}&page=1&pageSize=12`,
    ),
  getPerson: (personId: number) =>
    request<PersonProfile>(`/api/people/${personId}`),
  getStandings: (competitionId: number) =>
    request<StandingRow[]>(
      `/api/ranking/competitions/${competitionId}/standings`,
    ),
  getPlayerRankings: (competitionId: number, category = "goals") =>
    request<PlayerRanking[]>(
      `/api/ranking/players?competitionId=${competitionId}&category=${encodeURIComponent(category)}`,
    ),
  login: (userName: string, password: string) =>
    request<AdminLoginResponse>("/api/admin/login", {
      method: "POST",
      body: JSON.stringify({ userName, password }),
    }),
  getAdminMe: (token: string) =>
    request<AdminIdentity>("/api/admin/me", {
      headers: { Authorization: `Bearer ${token}` },
    }),
  getAdminUsers: (token: string) =>
    request<AdminIdentity[]>("/api/admin/users", {
      headers: { Authorization: `Bearer ${token}` },
    }),
  upsertAdminUser: (token: string, payload: AdminUserUpsertRequest) =>
    request<AdminIdentity>("/api/admin/users", {
      method: "POST",
      headers: { Authorization: `Bearer ${token}` },
      body: JSON.stringify(payload),
    }),
  getAdminChanges: (token: string) =>
    request<ChangeLogEntry[]>("/api/admin/changes", {
      headers: { Authorization: `Bearer ${token}` },
    }),
};

export function formatKickoff(value: string): string {
  return new Date(value).toLocaleString([], {
    dateStyle: "medium",
    timeStyle: "short",
  });
}

export function formatDate(value: string): string {
  return new Date(value).toLocaleDateString([], { dateStyle: "medium" });
}
