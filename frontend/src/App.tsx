import { useCallback, useEffect, useState, type FormEvent, type ReactNode } from "react";
import {
  BrowserRouter,
  Link,
  NavLink,
  Navigate,
  Route,
  Routes,
  useLocation,
  useNavigate,
  useParams,
} from "react-router-dom";
import { AuthProvider, useAuth } from "./auth/AuthContext";
import {
  api,
  formatDate,
  formatKickoff,
  type AdminIdentity,
  type AdminUserUpsertRequest,
  type ChangeLogEntry,
  type Competition,
  type CompetitionUpsertRequest,
  type Match,
  type MatchUpsertRequest,
  type Person,
  type PersonProfile,
  type PersonUpsertRequest,
  type PlayerRanking,
  type SeasonDetail,
  type Sport,
  type SportUpsertRequest,
  type Team,
  type TeamRoster,
  type TeamUpsertRequest,
} from "./lib/api";
import "./App.css";

interface DashboardData {
  sports: Sport[];
  competitions: Competition[];
  liveMatches: Match[];
  upcomingMatches: Match[];
  teams: Team[];
  people: Person[];
}

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <AppShell />
      </BrowserRouter>
    </AuthProvider>
  );
}

function AppShell() {
  const { isAuthenticated, session, logout } = useAuth();
  const [data, setData] = useState<DashboardData>({
    sports: [],
    competitions: [],
    liveMatches: [],
    upcomingMatches: [],
    teams: [],
    people: [],
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadData = useCallback(async () => {
    try {
      const [
        sports,
        competitions,
        liveMatches,
        upcomingMatches,
        teams,
        people,
      ] = await Promise.all([
        api.getSports(),
        api.getCompetitions(),
        api.getLiveMatches(),
        api.getUpcomingMatches(),
        api.getTeams(),
        api.searchPeople(""),
      ]);

      setData({
        sports: sports.items,
        competitions: competitions.items,
        liveMatches,
        upcomingMatches,
        teams: teams.items,
        people: people.items,
      });
    } catch (loadError) {
      setError(
        loadError instanceof Error
          ? loadError.message
          : "Unable to load the esports operations platform right now.",
      );
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void loadData();
  }, [loadData]);

  return (
    <div className="app-shell">
      <header className="topbar">
        <div className="brand-block">
          <p className="eyebrow">Esports tournament management platform</p>
          <h1>Tornois</h1>
        </div>

        <nav className="nav-links">
          <AppNavLink to="/">Home</AppNavLink>
          <AppNavLink to="/sports">Games</AppNavLink>
          <AppNavLink to="/competitions">Tournaments</AppNavLink>
          <AppNavLink to="/teams">Teams</AppNavLink>
          <AppNavLink to="/players">Players</AppNavLink>
          <AppNavLink to="/matches">Series</AppNavLink>
        </nav>

        <div className="auth-strip">
          {isAuthenticated ? (
            <>
              <span className="user-chip">
                {session?.user.displayName} · {session?.user.role}
              </span>
              <Link className="button-secondary" to="/admin">
                Dashboard
              </Link>
              <button className="button-ghost" onClick={logout} type="button">
                Logout
              </button>
            </>
          ) : (
            <Link className="button-secondary" to="/admin/login">
              Admin login
            </Link>
          )}
        </div>
      </header>

      <main className="layout">
        {loading ? (
          <StatePanel
            title="Loading tournament data"
            message="Fetching game titles, brackets, teams, streams, and rankings..."
          />
        ) : error ? (
          <StatePanel
            title="Backend not reachable"
            message={`${error} Start the API on http://localhost:5000 to enable tournament data.`}
          />
        ) : (
          <Routes>
            <Route path="/" element={<HomePage data={data} />} />
            <Route
              path="/sports"
              element={
                <SportsDirectoryPage
                  sports={data.sports}
                  competitions={data.competitions}
                />
              }
            />
            <Route
              path="/sports/:sportId"
              element={
                <SportDetailPage
                  sports={data.sports}
                  competitions={data.competitions}
                />
              }
            />
            <Route
              path="/competitions"
              element={
                <CompetitionsDirectoryPage competitions={data.competitions} />
              }
            />
            <Route
              path="/competitions/:competitionId"
              element={<CompetitionDetailPage />}
            />
            <Route
              path="/teams"
              element={<TeamsDirectoryPage teams={data.teams} />}
            />
            <Route path="/teams/:teamId" element={<TeamDetailPage />} />
            <Route
              path="/players"
              element={<PlayersDirectoryPage people={data.people} />}
            />
            <Route path="/players/:personId" element={<PlayerDetailPage />} />
            <Route
              path="/matches"
              element={
                <MatchesPage
                  liveMatches={data.liveMatches}
                  upcomingMatches={data.upcomingMatches}
                />
              }
            />
            <Route path="/matches/:matchId" element={<MatchDetailPage />} />
            <Route path="/admin/login" element={<AdminLoginPage />} />
            <Route
              path="/admin"
              element={
                <ProtectedRoute>
                  <AdminDashboardPage onDataChanged={loadData} />
                </ProtectedRoute>
              }
            />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>
        )}
      </main>
    </div>
  );
}

function AppNavLink({ to, children }: { to: string; children: ReactNode }) {
  return (
    <NavLink
      className={({ isActive }) => `nav-link${isActive ? " active" : ""}`}
      to={to}
      end={to === "/"}
    >
      {children}
    </NavLink>
  );
}

function HomePage({ data }: { data: DashboardData }) {
  const stats = [
    { label: "Game titles", value: data.sports.length },
    { label: "Tournaments", value: data.competitions.length },
    { label: "Live series", value: data.liveMatches.length },
    { label: "Registered players", value: data.people.length },
  ];

  return (
    <div className="page-stack">
      <section className="hero-panel">
        <div>
          <p className="eyebrow">Competitive gaming operations hub</p>
          <h2>
            Manage tournaments, rosters, streams, and live series in one place.
          </h2>
          <p className="muted-text">
            This MVP keeps the current ASP.NET + React architecture while
            repositioning Tornois as an esports tournament management platform
            for organizers and admins.
          </p>
        </div>
        <div className="hero-actions">
          <Link className="button-primary" to="/matches">
            View live series
          </Link>
          <Link className="button-secondary" to="/sports">
            Explore game titles
          </Link>
        </div>
        <div className="stats-grid">
          {stats.map((stat) => (
            <article className="stat-card" key={stat.label}>
              <span>{stat.label}</span>
              <strong>{stat.value}</strong>
            </article>
          ))}
        </div>
      </section>

      <section className="panel">
        <SectionHeading
          title="Live now"
          subtitle="Current esports series and stream-ready match blocks"
        />
        <div className="card-grid">
          {data.liveMatches.map((match) => (
            <MatchCard key={match.id} match={match} />
          ))}
        </div>
      </section>

      <section className="two-column">
        <div className="panel">
          <SectionHeading
            title="Game catalog"
            subtitle="Start from a title and drill down into its active tournament circuit"
          />
          <div className="card-grid compact-grid">
            {data.sports.map((sport) => (
              <Link
                className="panel-link"
                key={sport.id}
                to={`/sports/${sport.id}`}
              >
                <h3>{sport.name}</h3>
                <p className="muted-text">{sport.description}</p>
                <span className="chip">
                  {sport.isOlympic
                    ? "Publisher-backed circuit"
                    : "Open tournament ecosystem"}
                </span>
              </Link>
            ))}
          </div>
        </div>

        <div className="panel">
          <SectionHeading
            title="Upcoming series"
            subtitle="Next matchups scheduled in the tournament calendar"
          />
          <div className="stack-list">
            {data.upcomingMatches.map((match) => (
              <Link
                className="list-row"
                key={match.id}
                to={`/matches/${match.id}`}
              >
                <div>
                  <strong>
                    {match.homeTeam} vs {match.awayTeam}
                  </strong>
                  <p className="muted-text">{match.competition}</p>
                </div>
                <span>{formatKickoff(match.kickoffUtc)}</span>
              </Link>
            ))}
          </div>
        </div>
      </section>
    </div>
  );
}

function SportsDirectoryPage({
  sports,
  competitions,
}: {
  sports: Sport[];
  competitions: Competition[];
}) {
  return (
    <section className="panel">
      <SectionHeading
        title="Game titles"
        subtitle="Supported esports titles and the tournament circuits built around them"
      />
      <div className="card-grid">
        {sports.map((sport) => {
          const sportCompetitions = competitions.filter(
            (competition) => competition.sportId === sport.id,
          );
          return (
            <article className="detail-card" key={sport.id}>
              <h3>{sport.name}</h3>
              <p className="muted-text">{sport.description}</p>
              <div className="chip-row">
                {sportCompetitions.map((competition) => (
                  <Link
                    className="chip"
                    key={competition.id}
                    to={`/competitions/${competition.id}`}
                  >
                    {competition.name}
                  </Link>
                ))}
              </div>
            </article>
          );
        })}
      </div>
    </section>
  );
}

function SportDetailPage({
  sports,
  competitions,
}: {
  sports: Sport[];
  competitions: Competition[];
}) {
  const params = useParams();
  const sport = sports.find((entry) => entry.id === Number(params.sportId));

  if (!sport) {
    return (
      <StatePanel
        title="Game title not found"
        message="Pick a title from the directory to continue."
      />
    );
  }

  const relatedCompetitions = competitions.filter(
    (competition) => competition.sportId === sport.id,
  );

  return (
    <section className="panel">
      <SectionHeading title={sport.name} subtitle={sport.description} />
      <div className="card-grid">
        {relatedCompetitions.map((competition) => (
          <Link
            className="panel-link"
            key={competition.id}
            to={`/competitions/${competition.id}`}
          >
            <h3>{competition.name}</h3>
            <p className="muted-text">
              {competition.country} · {competition.format}
            </p>
          </Link>
        ))}
      </div>
    </section>
  );
}

function CompetitionsDirectoryPage({
  competitions,
}: {
  competitions: Competition[];
}) {
  return (
    <section className="panel">
      <SectionHeading
        title="Tournaments"
        subtitle="League stages, qualifiers, and international events currently available for browsing"
      />
      <div className="card-grid">
        {competitions.map((competition) => (
          <Link
            className="panel-link"
            key={competition.id}
            to={`/competitions/${competition.id}`}
          >
            <h3>{competition.name}</h3>
            <p className="muted-text">{competition.country}</p>
            <span className="chip">{competition.format}</span>
          </Link>
        ))}
      </div>
    </section>
  );
}

function CompetitionDetailPage() {
  const params = useParams();
  const competitionId = Number(params.competitionId);
  const [seasonDetail, setSeasonDetail] = useState<SeasonDetail | null>(null);
  const [rankings, setRankings] = useState<PlayerRanking[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      try {
        const seasons = await api.getCompetitionSeasons(competitionId);
        const currentSeason = seasons.items[0];
        if (!currentSeason) {
          throw new Error("No season data available for this competition.");
        }

        const [detail, playerRankings] = await Promise.all([
          api.getSeasonDetail(currentSeason.id),
          api.getPlayerRankings(competitionId),
        ]);

        setSeasonDetail(detail);
        setRankings(playerRankings);
      } catch (loadError) {
        setError(
          loadError instanceof Error
            ? loadError.message
            : "Unable to load competition details.",
        );
      } finally {
        setLoading(false);
      }
    };

    void load();
  }, [competitionId]);

  if (loading) {
    return (
      <StatePanel
        title="Loading tournament"
        message="Fetching stage standings and player rankings..."
      />
    );
  }

  if (error || !seasonDetail) {
    return (
      <StatePanel
        title="Tournament unavailable"
        message={error ?? "The requested tournament could not be found."}
      />
    );
  }

  return (
    <div className="page-stack">
      <section className="panel">
        <SectionHeading
          title={`${seasonDetail.competition.name} · ${seasonDetail.season.name}`}
          subtitle={`${seasonDetail.competition.country} · ${seasonDetail.competition.format}`}
        />
        <table className="data-table">
          <thead>
            <tr>
              <th>#</th>
              <th>Team</th>
              <th>P</th>
              <th>W</th>
              <th>D</th>
              <th>L</th>
              <th>Pts</th>
            </tr>
          </thead>
          <tbody>
            {seasonDetail.standings.map((row) => (
              <tr key={row.position}>
                <td>{row.position}</td>
                <td>{row.team}</td>
                <td>{row.played}</td>
                <td>{row.won}</td>
                <td>{row.drawn}</td>
                <td>{row.lost}</td>
                <td>{row.points}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>

      <section className="panel">
        <SectionHeading
          title="Player leaderboards"
          subtitle="Current top performers from this tournament"
        />
        <div className="stack-list">
          {rankings.map((entry) => (
            <div className="list-row" key={`${entry.person}-${entry.category}`}>
              <div>
                <strong>
                  #{entry.position} {entry.person}
                </strong>
                <p className="muted-text">
                  {entry.team} · {entry.category}
                </p>
              </div>
              <span className="chip">{entry.value}</span>
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}

function TeamsDirectoryPage({ teams }: { teams: Team[] }) {
  return (
    <section className="panel">
      <SectionHeading
        title="Teams"
        subtitle="Browse esports organizations and their active rosters"
      />
      <div className="card-grid">
        {teams.map((team) => (
          <Link className="panel-link" key={team.id} to={`/teams/${team.id}`}>
            <h3>{team.name}</h3>
            <p className="muted-text">
              {team.country} · {team.venue}
            </p>
            <span className="chip">Founded {team.founded}</span>
          </Link>
        ))}
      </div>
    </section>
  );
}

function TeamDetailPage() {
  const params = useParams();
  const [roster, setRoster] = useState<TeamRoster | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      try {
        setRoster(await api.getTeamRoster(Number(params.teamId)));
      } catch (loadError) {
        setError(
          loadError instanceof Error
            ? loadError.message
            : "Unable to load roster.",
        );
      } finally {
        setLoading(false);
      }
    };

    void load();
  }, [params.teamId]);

  if (loading) {
    return (
      <StatePanel
        title="Loading roster"
        message="Fetching players, staff roles, and team details..."
      />
    );
  }

  if (error || !roster) {
    return (
      <StatePanel
        title="Team unavailable"
        message={error ?? "The selected team could not be found."}
      />
    );
  }

  return (
    <section className="panel">
      <SectionHeading
        title={roster.team.name}
        subtitle={`${roster.team.country} · ${roster.team.venue}`}
      />
      <div className="card-grid">
        {roster.members.map((member) => (
          <Link
            className="panel-link"
            key={member.id}
            to={`/players/${member.id}`}
          >
            <h3>{member.fullName}</h3>
            <p className="muted-text">
              {member.role} · {member.nationality}
            </p>
            <span className="chip">#{member.shirtNumber ?? "—"}</span>
          </Link>
        ))}
      </div>
    </section>
  );
}

function PlayersDirectoryPage({ people }: { people: Person[] }) {
  return (
    <section className="panel">
      <SectionHeading
        title="Players & staff"
        subtitle="Search-ready directory of players, coaches, and tournament staff"
      />
      <div className="card-grid">
        {people.map((person) => (
          <Link
            className="panel-link"
            key={person.id}
            to={`/players/${person.id}`}
          >
            <h3>{person.fullName}</h3>
            <p className="muted-text">
              {person.role} · {person.nationality}
            </p>
            <span className="chip">Born {formatDate(person.birthDate)}</span>
          </Link>
        ))}
      </div>
    </section>
  );
}

function PlayerDetailPage() {
  const params = useParams();
  const [profile, setProfile] = useState<PersonProfile | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      try {
        setProfile(await api.getPerson(Number(params.personId)));
      } catch (loadError) {
        setError(
          loadError instanceof Error
            ? loadError.message
            : "Unable to load profile.",
        );
      } finally {
        setLoading(false);
      }
    };

    void load();
  }, [params.personId]);

  if (loading) {
    return (
      <StatePanel
        title="Loading player profile"
        message="Fetching biography, role, and roster details..."
      />
    );
  }

  if (error || !profile) {
    return (
      <StatePanel
        title="Player unavailable"
        message={error ?? "The selected person could not be found."}
      />
    );
  }

  return (
    <section className="panel">
      <SectionHeading
        title={profile.person.fullName}
        subtitle={`${profile.person.role} · ${profile.teamName}`}
      />
      <div className="info-grid">
        <div>
          <p className="muted-text">Nationality / region</p>
          <strong>{profile.person.nationality}</strong>
        </div>
        <div>
          <p className="muted-text">Birth date</p>
          <strong>{formatDate(profile.person.birthDate)}</strong>
        </div>
        <div>
          <p className="muted-text">Jersey / slot</p>
          <strong>{profile.person.shirtNumber ?? "—"}</strong>
        </div>
      </div>
      <p className="bio-copy">{profile.bio}</p>
    </section>
  );
}

function MatchesPage({
  liveMatches,
  upcomingMatches,
}: {
  liveMatches: Match[];
  upcomingMatches: Match[];
}) {
  return (
    <div className="page-stack">
      <section className="panel">
        <SectionHeading
          title="Live series"
          subtitle="Real-time scoreboard and tournament control feed"
        />
        <div className="card-grid">
          {liveMatches.map((match) => (
            <MatchCard key={match.id} match={match} />
          ))}
        </div>
      </section>

      <section className="panel">
        <SectionHeading
          title="Upcoming series"
          subtitle="Scheduled matchups and stage start times"
        />
        <div className="stack-list">
          {upcomingMatches.map((match) => (
            <Link
              className="list-row"
              key={match.id}
              to={`/matches/${match.id}`}
            >
              <div>
                <strong>
                  {match.homeTeam} vs {match.awayTeam}
                </strong>
                <p className="muted-text">{match.venue}</p>
              </div>
              <span>{formatKickoff(match.kickoffUtc)}</span>
            </Link>
          ))}
        </div>
      </section>
    </div>
  );
}

function MatchDetailPage() {
  const params = useParams();
  const [detail, setDetail] = useState<Awaited<
    ReturnType<typeof api.getMatchDetail>
  > | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      try {
        setDetail(await api.getMatchDetail(Number(params.matchId)));
      } catch (loadError) {
        setError(
          loadError instanceof Error
            ? loadError.message
            : "Unable to load match detail.",
        );
      } finally {
        setLoading(false);
      }
    };

    void load();
  }, [params.matchId]);

  if (loading) {
    return (
      <StatePanel
        title="Loading series detail"
        message="Fetching round timeline and live event data..."
      />
    );
  }

  if (error || !detail) {
    return (
      <StatePanel
        title="Series unavailable"
        message={error ?? "The selected series could not be found."}
      />
    );
  }

  const { match, events } = detail;

  return (
    <section className="panel">
      <SectionHeading
        title={`${match.homeTeam} ${match.homeScore} - ${match.awayScore} ${match.awayTeam}`}
        subtitle={`${match.competition} · ${match.status} · ${formatKickoff(match.kickoffUtc)}`}
      />
      <div className="stack-list">
        {events.map((event, index) => (
          <div className="list-row" key={`${event.subject}-${index}`}>
            <div>
              <strong>{event.subject}</strong>
              <p className="muted-text">{event.type}</p>
            </div>
            <span className="chip">{event.minute}'</span>
          </div>
        ))}
      </div>
    </section>
  );
}

function AdminLoginPage() {
  const { isAuthenticated, login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [userName, setUserName] = useState("superadmin");
  const [password, setPassword] = useState("Pass@123");
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  if (isAuthenticated) {
    return <Navigate to="/admin" replace />;
  }

  const onSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setSubmitting(true);
    setError(null);

    try {
      await login(userName, password);
      const target = ((location.state as { from?: string } | null)?.from ??
        "/admin") as string;
      navigate(target, { replace: true });
    } catch (loginError) {
      setError(
        loginError instanceof Error ? loginError.message : "Login failed.",
      );
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <section className="panel narrow-panel">
      <SectionHeading
        title="Admin login"
        subtitle="JWT authentication for tournament operations and staff access"
      />
      <form className="form-stack" onSubmit={onSubmit}>
        <label>
          <span>Username</span>
          <input
            className="input"
            onChange={(event) => setUserName(event.target.value)}
            value={userName}
          />
        </label>
        <label>
          <span>Password</span>
          <input
            className="input"
            onChange={(event) => setPassword(event.target.value)}
            type="password"
            value={password}
          />
        </label>
        {error ? <p className="error-text">{error}</p> : null}
        <button className="button-primary" disabled={submitting} type="submit">
          {submitting ? "Signing in..." : "Sign in"}
        </button>
      </form>
      <p className="muted-text">
        Demo seed users: `superadmin / Pass@123`, `editor / Editor@123`.
      </p>
    </section>
  );
}

type AdminTab = "overview" | "games" | "tournaments" | "teams" | "players" | "series" | "users";

function AdminDashboardPage({ onDataChanged }: { onDataChanged: () => Promise<void> }) {
  const { session } = useAuth();
  const [activeTab, setActiveTab] = useState<AdminTab>("overview");
  const [changes, setChanges] = useState<ChangeLogEntry[]>([]);
  const [admins, setAdmins] = useState<AdminIdentity[]>([]);
  const [managedSports, setManagedSports] = useState<Sport[]>([]);
  const [managedCompetitions, setManagedCompetitions] = useState<Competition[]>(
    [],
  );
  const [managedTeams, setManagedTeams] = useState<Team[]>([]);
  const [managedPeople, setManagedPeople] = useState<Person[]>([]);
  const [managedMatches, setManagedMatches] = useState<Match[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [formState, setFormState] = useState<AdminUserUpsertRequest>({
    userName: "",
    displayName: "",
    role: "readonly",
    password: "",
    isActive: true,
  });
  const [sportForm, setSportForm] = useState<
    { id: number | null } & SportUpsertRequest
  >({
    id: null,
    name: "",
    slug: "",
    description: "",
    isOlympic: false,
  });
  const [competitionForm, setCompetitionForm] = useState<
    { id: number | null } & CompetitionUpsertRequest
  >({
    id: null,
    sportId: 1,
    name: "",
    country: "",
    format: "League",
    isCup: false,
    seasonName: "2026",
    yearStart: 2026,
    yearEnd: 2026,
    isCurrent: true,
  });
  const [teamForm, setTeamForm] = useState<
    { id: number | null } & TeamUpsertRequest
  >({
    id: null,
    sportId: 1,
    name: "",
    shortName: "",
    country: "",
    venue: "",
    founded: 2020,
    badgeUrl: "",
  });
  const [personForm, setPersonForm] = useState<
    { id: number | null } & PersonUpsertRequest
  >({
    id: null,
    fullName: "",
    firstName: "",
    lastName: "",
    nationality: "",
    birthDate: "2000-01-01",
    primaryRole: "",
    bio: "",
    photoUrl: "",
    teamId: null,
    shirtNumber: null,
    squadRole: "Starter",
  });
  const [matchForm, setMatchForm] = useState<
    { id: number | null } & MatchUpsertRequest
  >({
    id: null,
    competitionId: 101,
    seasonId: null,
    homeTeamId: 1,
    awayTeamId: 2,
    kickoffUtc: toDateTimeLocalInputValue(
      new Date(Date.now() + 86_400_000).toISOString(),
    ),
    status: "Scheduled",
    homeScore: 0,
    awayScore: 0,
    venue: "",
  });

  const isSuperadmin = session?.user.role === "superadmin";

  const resetSportForm = () =>
    setSportForm({
      id: null,
      name: "",
      slug: "",
      description: "",
      isOlympic: false,
    });

  const resetCompetitionForm = () =>
    setCompetitionForm({
      id: null,
      sportId: managedSports[0]?.id ?? 1,
      name: "",
      country: "",
      format: "League",
      isCup: false,
      seasonName: "2026",
      yearStart: 2026,
      yearEnd: 2026,
      isCurrent: true,
    });

  const resetTeamForm = () =>
    setTeamForm({
      id: null,
      sportId: managedSports[0]?.id ?? 1,
      name: "",
      shortName: "",
      country: "",
      venue: "",
      founded: 2020,
      badgeUrl: "",
    });

  const resetPersonForm = () =>
    setPersonForm({
      id: null,
      fullName: "",
      firstName: "",
      lastName: "",
      nationality: "",
      birthDate: "2000-01-01",
      primaryRole: "",
      bio: "",
      photoUrl: "",
      teamId: null,
      shirtNumber: null,
      squadRole: "Starter",
    });

  const resetMatchForm = () =>
    setMatchForm({
      id: null,
      competitionId: managedCompetitions[0]?.id ?? 101,
      seasonId: null,
      homeTeamId: managedTeams[0]?.id ?? 1,
      awayTeamId: managedTeams[1]?.id ?? managedTeams[0]?.id ?? 1,
      kickoffUtc: toDateTimeLocalInputValue(
        new Date(Date.now() + 86_400_000).toISOString(),
      ),
      status: "Scheduled",
      homeScore: 0,
      awayScore: 0,
      venue: "",
    });

  const loadDashboard = async (showLoadingState = false) => {
    if (!session) {
      return;
    }

    if (showLoadingState) {
      setLoading(true);
    }

    try {
      const [
        auditEntries,
        adminUsers,
        sports,
        competitions,
        teams,
        people,
        liveMatches,
        upcomingMatches,
      ] = await Promise.all([
        api.getAdminChanges(session.token),
        isSuperadmin ? api.getAdminUsers(session.token) : Promise.resolve([]),
        api.getSports(),
        api.getCompetitions(),
        api.getTeams(),
        api.searchPeople(""),
        api.getLiveMatches(),
        api.getUpcomingMatches(),
      ]);

      setChanges(auditEntries);
      setAdmins(adminUsers);
      setManagedSports(sports.items);
      setManagedCompetitions(competitions.items);
      setManagedTeams(teams.items);
      setManagedPeople(people.items);
      setManagedMatches(mergeMatches(liveMatches, upcomingMatches));
    } catch (loadError) {
      setError(
        loadError instanceof Error
          ? loadError.message
          : "Unable to load tournament management data.",
      );
    } finally {
      if (showLoadingState) {
        setLoading(false);
      }
    }
  };

  useEffect(() => {
    void loadDashboard(true);
  }, [isSuperadmin, session]);

  if (!session) {
    return null;
  }

  const runMutation = async (
    action: () => Promise<void>,
    message: string,
    reset?: () => void,
  ) => {
    setError(null);
    setSuccessMessage(null);

    try {
      await action();
      if (reset) {
        reset();
      }
      await loadDashboard(false);
      await onDataChanged();
      setSuccessMessage(message);
    } catch (mutationError) {
      setError(
        mutationError instanceof Error
          ? mutationError.message
          : "Unable to save tournament changes.",
      );
    }
  };

  const handleCreateAdmin = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    await runMutation(
      async () => {
        const createdUser = await api.upsertAdminUser(session.token, formState);
        setAdmins((current) => {
          const others = current.filter(
            (entry) => entry.userName !== createdUser.userName,
          );
          return [...others, createdUser].sort((left, right) =>
            left.userName.localeCompare(right.userName),
          );
        });
      },
      `Saved admin user ${formState.userName}.`,
      () =>
        setFormState({
          userName: "",
          displayName: "",
          role: "readonly",
          password: "",
          isActive: true,
        }),
    );
  };

  const handleSaveSport = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const payload: SportUpsertRequest = {
      name: sportForm.name,
      slug: sportForm.slug,
      description: sportForm.description,
      isOlympic: sportForm.isOlympic,
    };

    await runMutation(
      async () => {
        if (sportForm.id) {
          await api.updateSport(session.token, sportForm.id, payload);
        } else {
          await api.createSport(session.token, payload);
        }
      },
      `Saved game title ${sportForm.name}.`,
      resetSportForm,
    );
  };

  const handleSaveCompetition = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const payload: CompetitionUpsertRequest = {
      sportId: competitionForm.sportId,
      name: competitionForm.name,
      country: competitionForm.country,
      format: competitionForm.format,
      isCup: competitionForm.isCup,
      seasonName: competitionForm.seasonName,
      yearStart: competitionForm.yearStart,
      yearEnd: competitionForm.yearEnd,
      isCurrent: competitionForm.isCurrent,
    };

    await runMutation(
      async () => {
        if (competitionForm.id) {
          await api.updateCompetition(
            session.token,
            competitionForm.id,
            payload,
          );
        } else {
          await api.createCompetition(session.token, payload);
        }
      },
      `Saved tournament ${competitionForm.name}.`,
      resetCompetitionForm,
    );
  };

  const handleSaveTeam = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const payload: TeamUpsertRequest = {
      sportId: teamForm.sportId,
      name: teamForm.name,
      shortName: teamForm.shortName,
      country: teamForm.country,
      venue: teamForm.venue,
      founded: teamForm.founded,
      badgeUrl: teamForm.badgeUrl,
    };

    await runMutation(
      async () => {
        if (teamForm.id) {
          await api.updateTeam(session.token, teamForm.id, payload);
        } else {
          await api.createTeam(session.token, payload);
        }
      },
      `Saved team ${teamForm.name}.`,
      resetTeamForm,
    );
  };

  const handleSavePerson = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const payload: PersonUpsertRequest = {
      fullName: personForm.fullName,
      firstName: personForm.firstName,
      lastName: personForm.lastName,
      nationality: personForm.nationality,
      birthDate: personForm.birthDate,
      primaryRole: personForm.primaryRole,
      bio: personForm.bio,
      photoUrl: personForm.photoUrl,
      teamId: personForm.teamId,
      shirtNumber: personForm.shirtNumber,
      squadRole: personForm.squadRole,
    };

    await runMutation(
      async () => {
        if (personForm.id) {
          await api.updatePerson(session.token, personForm.id, payload);
        } else {
          await api.createPerson(session.token, payload);
        }
      },
      `Saved profile ${personForm.fullName}.`,
      resetPersonForm,
    );
  };

  const handleSaveMatch = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const payload: MatchUpsertRequest = {
      competitionId: matchForm.competitionId,
      seasonId: matchForm.seasonId,
      homeTeamId: matchForm.homeTeamId,
      awayTeamId: matchForm.awayTeamId,
      kickoffUtc: new Date(matchForm.kickoffUtc).toISOString(),
      status: matchForm.status,
      homeScore: matchForm.homeScore,
      awayScore: matchForm.awayScore,
      venue: matchForm.venue,
    };

    await runMutation(
      async () => {
        if (matchForm.id) {
          await api.updateMatch(session.token, matchForm.id, payload);
        } else {
          await api.createMatch(session.token, payload);
        }
      },
      `Saved series schedule and scoreline.`,
      resetMatchForm,
    );
  };

  const handleEditCompetition = async (competition: Competition) => {
    try {
      const seasons = await api.getCompetitionSeasons(competition.id);
      const season = seasons.items[0];
      setCompetitionForm({
        id: competition.id,
        sportId: competition.sportId,
        name: competition.name,
        country: competition.country,
        format: competition.format,
        isCup: competition.isCup,
        seasonName: season?.name ?? "2026",
        yearStart: season?.yearStart ?? 2026,
        yearEnd: season?.yearEnd ?? 2026,
        isCurrent: season?.isCurrent ?? true,
      });
    } catch (loadError) {
      setError(
        loadError instanceof Error
          ? loadError.message
          : "Unable to load tournament details.",
      );
    }
  };

  const handleEditPerson = async (person: Person) => {
    try {
      const profile = await api.getPerson(person.id);
      setPersonForm({
        id: person.id,
        fullName: profile.person.fullName,
        firstName: profile.person.fullName.split(" ")[0] ?? "",
        lastName: profile.person.fullName.split(" ").slice(1).join(" ") ?? "",
        nationality: profile.person.nationality,
        birthDate: profile.person.birthDate,
        primaryRole: profile.person.role,
        bio: profile.bio,
        photoUrl: profile.person.photoUrl,
        teamId: profile.person.teamId > 0 ? profile.person.teamId : null,
        shirtNumber: profile.person.shirtNumber,
        squadRole: "Starter",
      });
    } catch (loadError) {
      setError(
        loadError instanceof Error
          ? loadError.message
          : "Unable to load roster profile.",
      );
    }
  };

  return (
    <div className="page-stack">
      <section className="panel">
        <SectionHeading
          title="Admin dashboard"
          subtitle="Protected operational overview for tournament organizers and staff"
        />
        <div className="info-grid">
          <div>
            <p className="muted-text">Signed in as</p>
            <strong>{session.user.displayName}</strong>
          </div>
          <div>
            <p className="muted-text">Role</p>
            <strong>{session.user.role}</strong>
          </div>
          <div>
            <p className="muted-text">Token expires</p>
            <strong>{formatKickoff(session.expiresAtUtc)}</strong>
          </div>
        </div>
        {successMessage ? <p className="muted-text">{successMessage}</p> : null}
        {error ? <p className="error-text">{error}</p> : null}
        <nav className="admin-tabs">
          {([
            ["overview", "Overview"],
            ["games", "Games"],
            ["tournaments", "Tournaments"],
            ["teams", "Teams"],
            ["players", "Players"],
            ["series", "Series"],
            ...(isSuperadmin ? [["users", "Users"]] : []),
          ] as [AdminTab, string][]).map(([id, label]) => (
            <button
              key={id}
              className={`tab-button${activeTab === id ? " tab-active" : ""}`}
              onClick={() => setActiveTab(id)}
              type="button"
            >
              {label}
            </button>
          ))}
        </nav>
      </section>

      {activeTab === "users" && isSuperadmin ? (
        <section className="panel">
          <SectionHeading
            title="Admin user management"
            subtitle="Create or update organizer, editor, and viewer accounts"
          />
          <div className="two-column">
            <form className="form-stack" onSubmit={handleCreateAdmin}>
              <label>
                <span>Username</span>
                <input
                  className="input"
                  value={formState.userName}
                  onChange={(event) =>
                    setFormState((current) => ({
                      ...current,
                      userName: event.target.value,
                    }))
                  }
                />
              </label>
              <label>
                <span>Display name</span>
                <input
                  className="input"
                  value={formState.displayName}
                  onChange={(event) =>
                    setFormState((current) => ({
                      ...current,
                      displayName: event.target.value,
                    }))
                  }
                />
              </label>
              <label>
                <span>Role</span>
                <select
                  className="input"
                  value={formState.role}
                  onChange={(event) =>
                    setFormState((current) => ({
                      ...current,
                      role: event.target
                        .value as AdminUserUpsertRequest["role"],
                    }))
                  }
                >
                  <option value="superadmin">superadmin</option>
                  <option value="editor">editor</option>
                  <option value="readonly">readonly</option>
                </select>
              </label>
              <label>
                <span>Password</span>
                <input
                  className="input"
                  type="password"
                  value={formState.password}
                  onChange={(event) =>
                    setFormState((current) => ({
                      ...current,
                      password: event.target.value,
                    }))
                  }
                />
              </label>
              <button className="button-primary" type="submit">
                Save admin user
              </button>
            </form>

            <div className="stack-list">
              {admins.map((admin) => (
                <div className="list-row" key={admin.userName}>
                  <div>
                    <strong>{admin.displayName}</strong>
                    <p className="muted-text">{admin.userName}</p>
                  </div>
                  <span className="chip">{admin.role}</span>
                </div>
              ))}
            </div>
          </div>
        </section>
      ) : null}

      {activeTab !== "overview" && activeTab !== "users" && (
        loading ? (
          <StatePanel title="Loading management tools" message="Fetching data..." />
        ) : (
          <>
            {activeTab === "games" && (
            <ManagementCard
              title="Game titles"
              subtitle="Create and maintain the esports titles available on the platform"
            >
              <form className="form-stack" onSubmit={handleSaveSport}>
                <label>
                  <span>Name</span>
                  <input
                    className="input"
                    value={sportForm.name}
                    onChange={(event) =>
                      setSportForm((current) => ({
                        ...current,
                        name: event.target.value,
                      }))
                    }
                  />
                </label>
                <label>
                  <span>Slug</span>
                  <input
                    className="input"
                    value={sportForm.slug}
                    onChange={(event) =>
                      setSportForm((current) => ({
                        ...current,
                        slug: event.target.value.toLowerCase(),
                      }))
                    }
                  />
                </label>
                <label>
                  <span>Description</span>
                  <textarea
                    className="input"
                    rows={3}
                    value={sportForm.description}
                    onChange={(event) =>
                      setSportForm((current) => ({
                        ...current,
                        description: event.target.value,
                      }))
                    }
                  />
                </label>
                <label>
                  <span>Publisher-backed circuit</span>
                  <select
                    className="input"
                    value={sportForm.isOlympic ? "yes" : "no"}
                    onChange={(event) =>
                      setSportForm((current) => ({
                        ...current,
                        isOlympic: event.target.value === "yes",
                      }))
                    }
                  >
                    <option value="yes">Yes</option>
                    <option value="no">No</option>
                  </select>
                </label>
                <div className="inline-actions">
                  <button className="button-primary" type="submit">
                    {sportForm.id ? "Update title" : "Create title"}
                  </button>
                  <button
                    className="button-ghost"
                    onClick={resetSportForm}
                    type="button"
                  >
                    Clear
                  </button>
                </div>
              </form>
              <div className="stack-list">
                {managedSports.map((sport) => (
                  <div className="list-row" key={sport.id}>
                    <div>
                      <strong>{sport.name}</strong>
                      <p className="muted-text">{sport.description}</p>
                    </div>
                    <div className="inline-actions">
                      <button
                        className="button-secondary"
                        onClick={() =>
                          setSportForm({
                            id: sport.id,
                            name: sport.name,
                            slug: sport.slug,
                            description: sport.description,
                            isOlympic: sport.isOlympic,
                          })
                        }
                        type="button"
                      >
                        Edit
                      </button>
                      <button
                        className="button-danger"
                        onClick={() => {
                          if (!window.confirm(`Delete ${sport.name}?`)) {
                            return;
                          }

                          void runMutation(
                            () => api.deleteSport(session.token, sport.id),
                            `Deleted ${sport.name}.`,
                            resetSportForm,
                          );
                        }}
                        type="button"
                      >
                        Delete
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            </ManagementCard>
            )}

            {activeTab === "tournaments" && (
            <ManagementCard
              title="Tournaments"
              subtitle="Manage leagues, stages, and the current competitive season"
            >
              <form className="form-stack" onSubmit={handleSaveCompetition}>
                <label>
                  <span>Game title</span>
                  <select
                    className="input"
                    value={competitionForm.sportId}
                    onChange={(event) =>
                      setCompetitionForm((current) => ({
                        ...current,
                        sportId: Number(event.target.value),
                      }))
                    }
                  >
                    {managedSports.map((sport) => (
                      <option key={sport.id} value={sport.id}>
                        {sport.name}
                      </option>
                    ))}
                  </select>
                </label>
                <label>
                  <span>Tournament name</span>
                  <input
                    className="input"
                    value={competitionForm.name}
                    onChange={(event) =>
                      setCompetitionForm((current) => ({
                        ...current,
                        name: event.target.value,
                      }))
                    }
                  />
                </label>
                <div className="two-column single-entity-grid">
                  <label>
                    <span>Region</span>
                    <input
                      className="input"
                      value={competitionForm.country}
                      onChange={(event) =>
                        setCompetitionForm((current) => ({
                          ...current,
                          country: event.target.value,
                        }))
                      }
                    />
                  </label>
                  <label>
                    <span>Format</span>
                    <input
                      className="input"
                      value={competitionForm.format}
                      onChange={(event) =>
                        setCompetitionForm((current) => ({
                          ...current,
                          format: event.target.value,
                        }))
                      }
                    />
                  </label>
                </div>
                <div className="two-column single-entity-grid">
                  <label>
                    <span>Season / stage</span>
                    <input
                      className="input"
                      value={competitionForm.seasonName}
                      onChange={(event) =>
                        setCompetitionForm((current) => ({
                          ...current,
                          seasonName: event.target.value,
                        }))
                      }
                    />
                  </label>
                  <label>
                    <span>Is cup event</span>
                    <select
                      className="input"
                      value={competitionForm.isCup ? "yes" : "no"}
                      onChange={(event) =>
                        setCompetitionForm((current) => ({
                          ...current,
                          isCup: event.target.value === "yes",
                        }))
                      }
                    >
                      <option value="no">No</option>
                      <option value="yes">Yes</option>
                    </select>
                  </label>
                </div>
                <div className="two-column single-entity-grid">
                  <label>
                    <span>Start year</span>
                    <input
                      className="input"
                      type="number"
                      value={competitionForm.yearStart}
                      onChange={(event) =>
                        setCompetitionForm((current) => ({
                          ...current,
                          yearStart: Number(event.target.value),
                        }))
                      }
                    />
                  </label>
                  <label>
                    <span>End year</span>
                    <input
                      className="input"
                      type="number"
                      value={competitionForm.yearEnd}
                      onChange={(event) =>
                        setCompetitionForm((current) => ({
                          ...current,
                          yearEnd: Number(event.target.value),
                        }))
                      }
                    />
                  </label>
                </div>
                <div className="inline-actions">
                  <button className="button-primary" type="submit">
                    {competitionForm.id
                      ? "Update tournament"
                      : "Create tournament"}
                  </button>
                  <button
                    className="button-ghost"
                    onClick={resetCompetitionForm}
                    type="button"
                  >
                    Clear
                  </button>
                </div>
              </form>
              <div className="stack-list">
                {managedCompetitions.map((competition) => (
                  <div className="list-row" key={competition.id}>
                    <div>
                      <strong>{competition.name}</strong>
                      <p className="muted-text">
                        {competition.sport} · {competition.country} ·{" "}
                        {competition.format}
                      </p>
                    </div>
                    <div className="inline-actions">
                      <button
                        className="button-secondary"
                        onClick={() => {
                          void handleEditCompetition(competition);
                        }}
                        type="button"
                      >
                        Edit
                      </button>
                      <button
                        className="button-danger"
                        onClick={() => {
                          if (!window.confirm(`Delete ${competition.name}?`)) {
                            return;
                          }

                          void runMutation(
                            () =>
                              api.deleteCompetition(
                                session.token,
                                competition.id,
                              ),
                            `Deleted ${competition.name}.`,
                            resetCompetitionForm,
                          );
                        }}
                        type="button"
                      >
                        Delete
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            </ManagementCard>
            )}

            {activeTab === "teams" && (
            <ManagementCard
              title="Teams"
              subtitle="Register organizations, brand info, and team headquarters"
            >
              <form className="form-stack" onSubmit={handleSaveTeam}>
                <label>
                  <span>Game title</span>
                  <select
                    className="input"
                    value={teamForm.sportId}
                    onChange={(event) =>
                      setTeamForm((current) => ({
                        ...current,
                        sportId: Number(event.target.value),
                      }))
                    }
                  >
                    {managedSports.map((sport) => (
                      <option key={sport.id} value={sport.id}>
                        {sport.name}
                      </option>
                    ))}
                  </select>
                </label>
                <div className="two-column single-entity-grid">
                  <label>
                    <span>Team name</span>
                    <input
                      className="input"
                      value={teamForm.name}
                      onChange={(event) =>
                        setTeamForm((current) => ({
                          ...current,
                          name: event.target.value,
                        }))
                      }
                    />
                  </label>
                  <label>
                    <span>Short name</span>
                    <input
                      className="input"
                      value={teamForm.shortName}
                      onChange={(event) =>
                        setTeamForm((current) => ({
                          ...current,
                          shortName: event.target.value,
                        }))
                      }
                    />
                  </label>
                </div>
                <div className="two-column single-entity-grid">
                  <label>
                    <span>Country / region</span>
                    <input
                      className="input"
                      value={teamForm.country}
                      onChange={(event) =>
                        setTeamForm((current) => ({
                          ...current,
                          country: event.target.value,
                        }))
                      }
                    />
                  </label>
                  <label>
                    <span>Founded</span>
                    <input
                      className="input"
                      type="number"
                      value={teamForm.founded}
                      onChange={(event) =>
                        setTeamForm((current) => ({
                          ...current,
                          founded: Number(event.target.value),
                        }))
                      }
                    />
                  </label>
                </div>
                <label>
                  <span>Base / venue</span>
                  <input
                    className="input"
                    value={teamForm.venue}
                    onChange={(event) =>
                      setTeamForm((current) => ({
                        ...current,
                        venue: event.target.value,
                      }))
                    }
                  />
                </label>
                <label>
                  <span>Badge URL</span>
                  <input
                    className="input"
                    value={teamForm.badgeUrl}
                    onChange={(event) =>
                      setTeamForm((current) => ({
                        ...current,
                        badgeUrl: event.target.value,
                      }))
                    }
                  />
                </label>
                <div className="inline-actions">
                  <button className="button-primary" type="submit">
                    {teamForm.id ? "Update team" : "Create team"}
                  </button>
                  <button
                    className="button-ghost"
                    onClick={resetTeamForm}
                    type="button"
                  >
                    Clear
                  </button>
                </div>
              </form>
              <div className="stack-list">
                {managedTeams.map((team) => (
                  <div className="list-row" key={team.id}>
                    <div>
                      <strong>{team.name}</strong>
                      <p className="muted-text">
                        {team.country} · {team.venue}
                      </p>
                    </div>
                    <div className="inline-actions">
                      <button
                        className="button-secondary"
                        onClick={() =>
                          setTeamForm({
                            id: team.id,
                            sportId: team.sportId,
                            name: team.name,
                            shortName: team.shortName,
                            country: team.country,
                            venue: team.venue,
                            founded: team.founded,
                            badgeUrl: team.badgeUrl,
                          })
                        }
                        type="button"
                      >
                        Edit
                      </button>
                      <button
                        className="button-danger"
                        onClick={() => {
                          if (!window.confirm(`Delete ${team.name}?`)) {
                            return;
                          }

                          void runMutation(
                            () => api.deleteTeam(session.token, team.id),
                            `Deleted ${team.name}.`,
                            resetTeamForm,
                          );
                        }}
                        type="button"
                      >
                        Delete
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            </ManagementCard>
            )}

            {activeTab === "players" && (
            <ManagementCard
              title="Players & staff"
              subtitle="Manage roster profiles, roles, and team assignments"
            >
              <form className="form-stack" onSubmit={handleSavePerson}>
                <label>
                  <span>Full name</span>
                  <input
                    className="input"
                    value={personForm.fullName}
                    onChange={(event) =>
                      setPersonForm((current) => ({
                        ...current,
                        fullName: event.target.value,
                      }))
                    }
                  />
                </label>
                <div className="two-column single-entity-grid">
                  <label>
                    <span>Primary role</span>
                    <input
                      className="input"
                      value={personForm.primaryRole}
                      onChange={(event) =>
                        setPersonForm((current) => ({
                          ...current,
                          primaryRole: event.target.value,
                        }))
                      }
                    />
                  </label>
                  <label>
                    <span>Nationality / region</span>
                    <input
                      className="input"
                      value={personForm.nationality}
                      onChange={(event) =>
                        setPersonForm((current) => ({
                          ...current,
                          nationality: event.target.value,
                        }))
                      }
                    />
                  </label>
                </div>
                <div className="two-column single-entity-grid">
                  <label>
                    <span>Birth date</span>
                    <input
                      className="input"
                      type="date"
                      value={personForm.birthDate}
                      onChange={(event) =>
                        setPersonForm((current) => ({
                          ...current,
                          birthDate: event.target.value,
                        }))
                      }
                    />
                  </label>
                  <label>
                    <span>Assigned team</span>
                    <select
                      className="input"
                      value={personForm.teamId ?? ""}
                      onChange={(event) =>
                        setPersonForm((current) => ({
                          ...current,
                          teamId: event.target.value
                            ? Number(event.target.value)
                            : null,
                        }))
                      }
                    >
                      <option value="">Unassigned</option>
                      {managedTeams.map((team) => (
                        <option key={team.id} value={team.id}>
                          {team.name}
                        </option>
                      ))}
                    </select>
                  </label>
                </div>
                <div className="two-column single-entity-grid">
                  <label>
                    <span>Squad role</span>
                    <input
                      className="input"
                      value={personForm.squadRole}
                      onChange={(event) =>
                        setPersonForm((current) => ({
                          ...current,
                          squadRole: event.target.value,
                        }))
                      }
                    />
                  </label>
                  <label>
                    <span>Jersey / slot</span>
                    <input
                      className="input"
                      type="number"
                      value={personForm.shirtNumber ?? ""}
                      onChange={(event) =>
                        setPersonForm((current) => ({
                          ...current,
                          shirtNumber: event.target.value
                            ? Number(event.target.value)
                            : null,
                        }))
                      }
                    />
                  </label>
                </div>
                <label>
                  <span>Bio</span>
                  <textarea
                    className="input"
                    rows={3}
                    value={personForm.bio}
                    onChange={(event) =>
                      setPersonForm((current) => ({
                        ...current,
                        bio: event.target.value,
                      }))
                    }
                  />
                </label>
                <div className="inline-actions">
                  <button className="button-primary" type="submit">
                    {personForm.id ? "Update profile" : "Create profile"}
                  </button>
                  <button
                    className="button-ghost"
                    onClick={resetPersonForm}
                    type="button"
                  >
                    Clear
                  </button>
                </div>
              </form>
              <div className="stack-list">
                {managedPeople.map((person) => (
                  <div className="list-row" key={person.id}>
                    <div>
                      <strong>{person.fullName}</strong>
                      <p className="muted-text">
                        {person.role} · {person.nationality}
                      </p>
                    </div>
                    <div className="inline-actions">
                      <button
                        className="button-secondary"
                        onClick={() => {
                          void handleEditPerson(person);
                        }}
                        type="button"
                      >
                        Edit
                      </button>
                      <button
                        className="button-danger"
                        onClick={() => {
                          if (!window.confirm(`Delete ${person.fullName}?`)) {
                            return;
                          }

                          void runMutation(
                            () => api.deletePerson(session.token, person.id),
                            `Deleted ${person.fullName}.`,
                            resetPersonForm,
                          );
                        }}
                        type="button"
                      >
                        Delete
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            </ManagementCard>
            )}

            {activeTab === "series" && (
            <ManagementCard
              title="Series scheduler"
              subtitle="Create and update scheduled or live series for tournament operations"
            >
              <form className="form-stack" onSubmit={handleSaveMatch}>
                <label>
                  <span>Tournament</span>
                  <select
                    className="input"
                    value={matchForm.competitionId}
                    onChange={(event) =>
                      setMatchForm((current) => ({
                        ...current,
                        competitionId: Number(event.target.value),
                      }))
                    }
                  >
                    {managedCompetitions.map((competition) => (
                      <option key={competition.id} value={competition.id}>
                        {competition.name}
                      </option>
                    ))}
                  </select>
                </label>
                <div className="two-column single-entity-grid">
                  <label>
                    <span>Home team</span>
                    <select
                      className="input"
                      value={matchForm.homeTeamId}
                      onChange={(event) =>
                        setMatchForm((current) => ({
                          ...current,
                          homeTeamId: Number(event.target.value),
                        }))
                      }
                    >
                      {managedTeams.map((team) => (
                        <option key={team.id} value={team.id}>
                          {team.name}
                        </option>
                      ))}
                    </select>
                  </label>
                  <label>
                    <span>Away team</span>
                    <select
                      className="input"
                      value={matchForm.awayTeamId}
                      onChange={(event) =>
                        setMatchForm((current) => ({
                          ...current,
                          awayTeamId: Number(event.target.value),
                        }))
                      }
                    >
                      {managedTeams.map((team) => (
                        <option key={team.id} value={team.id}>
                          {team.name}
                        </option>
                      ))}
                    </select>
                  </label>
                </div>
                <div className="two-column single-entity-grid">
                  <label>
                    <span>Kickoff / start time</span>
                    <input
                      className="input"
                      type="datetime-local"
                      value={matchForm.kickoffUtc}
                      onChange={(event) =>
                        setMatchForm((current) => ({
                          ...current,
                          kickoffUtc: event.target.value,
                        }))
                      }
                    />
                  </label>
                  <label>
                    <span>Status</span>
                    <select
                      className="input"
                      value={matchForm.status}
                      onChange={(event) =>
                        setMatchForm((current) => ({
                          ...current,
                          status: event.target.value,
                        }))
                      }
                    >
                      <option value="Scheduled">Scheduled</option>
                      <option value="Live">Live</option>
                      <option value="Completed">Completed</option>
                      <option value="Paused">Paused</option>
                    </select>
                  </label>
                </div>
                <div className="two-column single-entity-grid">
                  <label>
                    <span>Home score</span>
                    <input
                      className="input"
                      type="number"
                      value={matchForm.homeScore}
                      onChange={(event) =>
                        setMatchForm((current) => ({
                          ...current,
                          homeScore: Number(event.target.value),
                        }))
                      }
                    />
                  </label>
                  <label>
                    <span>Away score</span>
                    <input
                      className="input"
                      type="number"
                      value={matchForm.awayScore}
                      onChange={(event) =>
                        setMatchForm((current) => ({
                          ...current,
                          awayScore: Number(event.target.value),
                        }))
                      }
                    />
                  </label>
                </div>
                <label>
                  <span>Venue / stream stage</span>
                  <input
                    className="input"
                    value={matchForm.venue}
                    onChange={(event) =>
                      setMatchForm((current) => ({
                        ...current,
                        venue: event.target.value,
                      }))
                    }
                  />
                </label>
                <div className="inline-actions">
                  <button className="button-primary" type="submit">
                    {matchForm.id ? "Update series" : "Create series"}
                  </button>
                  <button
                    className="button-ghost"
                    onClick={resetMatchForm}
                    type="button"
                  >
                    Clear
                  </button>
                </div>
              </form>
              <div className="stack-list">
                {managedMatches.map((match) => (
                  <div className="list-row" key={match.id}>
                    <div>
                      <strong>
                        {match.homeTeam} vs {match.awayTeam}
                      </strong>
                      <p className="muted-text">
                        {match.competition} · {match.status} ·{" "}
                        {formatKickoff(match.kickoffUtc)}
                      </p>
                    </div>
                    <div className="inline-actions">
                      <button
                        className="button-secondary"
                        onClick={() =>
                          setMatchForm({
                            id: match.id,
                            competitionId: match.competitionId,
                            seasonId: match.seasonId,
                            homeTeamId:
                              managedTeams.find(
                                (team) => team.name === match.homeTeam,
                              )?.id ??
                              managedTeams[0]?.id ??
                              1,
                            awayTeamId:
                              managedTeams.find(
                                (team) => team.name === match.awayTeam,
                              )?.id ??
                              managedTeams[1]?.id ??
                              managedTeams[0]?.id ??
                              1,
                            kickoffUtc: toDateTimeLocalInputValue(
                              match.kickoffUtc,
                            ),
                            status: match.status,
                            homeScore: match.homeScore,
                            awayScore: match.awayScore,
                            venue: match.venue,
                          })
                        }
                        type="button"
                      >
                        Edit
                      </button>
                      <button
                        className="button-danger"
                        onClick={() => {
                          if (
                            !window.confirm(
                              `Delete ${match.homeTeam} vs ${match.awayTeam}?`,
                            )
                          ) {
                            return;
                          }

                          void runMutation(
                            () => api.deleteMatch(session.token, match.id),
                            `Deleted ${match.homeTeam} vs ${match.awayTeam}.`,
                            resetMatchForm,
                          );
                        }}
                        type="button"
                      >
                        Delete
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            </ManagementCard>
            )}
          </>
        )
      )}

      {activeTab === "overview" && (
      <section className="panel">
        <SectionHeading
          title="Recent sync and audit activity"
          subtitle="Roster, stream, and schedule updates coming from the backend"
        />
        {loading ? (
          <p className="muted-text">Loading audit entries...</p>
        ) : (
          <div className="stack-list">
            {changes.map((entry) => (
              <div className="list-row" key={entry.id}>
                <div>
                  <strong>
                    {entry.action} · {entry.entityName}
                  </strong>
                  <p className="muted-text">{entry.summary}</p>
                </div>
                <span>{formatKickoff(entry.timestamp)}</span>
              </div>
            ))}
          </div>
        )}
      </section>
      )}
    </div>
  );
}

function ManagementCard({
  title,
  subtitle,
  children,
}: {
  title: string;
  subtitle: string;
  children: ReactNode;
}) {
  return (
    <article className="panel management-card">
      <h3>{title}</h3>
      <p className="muted-text">{subtitle}</p>
      <div className="page-stack management-card-body">{children}</div>
    </article>
  );
}

function toDateTimeLocalInputValue(value: string): string {
  const date = new Date(value);
  const offset = date.getTimezoneOffset();
  return new Date(date.getTime() - offset * 60_000).toISOString().slice(0, 16);
}

function mergeMatches(liveMatches: Match[], upcomingMatches: Match[]) {
  const ordered = new Map<number, Match>();
  [...liveMatches, ...upcomingMatches]
    .sort(
      (left, right) =>
        new Date(left.kickoffUtc).getTime() -
        new Date(right.kickoffUtc).getTime(),
    )
    .forEach((match) => {
      ordered.set(match.id, match);
    });

  return Array.from(ordered.values());
}

function ProtectedRoute({ children }: { children: ReactNode }) {
  const { isAuthenticated, isLoading } = useAuth();
  const location = useLocation();

  if (isLoading) {
    return (
      <StatePanel
        title="Checking session"
        message="Verifying your admin token..."
      />
    );
  }

  if (!isAuthenticated) {
    return (
      <Navigate to="/admin/login" replace state={{ from: location.pathname }} />
    );
  }

  return <>{children}</>;
}

function MatchCard({ match }: { match: Match }) {
  return (
    <Link className="panel-link" to={`/matches/${match.id}`}>
      <div className="match-header">
        <span className="chip">{match.status}</span>
        <span className="muted-text">{match.competition}</span>
      </div>
      <h3>
        {match.homeTeam} vs {match.awayTeam}
      </h3>
      <p className="score-line">
        {match.homeScore} - {match.awayScore}
      </p>
      <p className="muted-text">{formatKickoff(match.kickoffUtc)}</p>
    </Link>
  );
}

function SectionHeading({
  title,
  subtitle,
}: {
  title: string;
  subtitle: string;
}) {
  return (
    <div className="section-heading">
      <h2>{title}</h2>
      <p className="muted-text">{subtitle}</p>
    </div>
  );
}

function StatePanel({ title, message }: { title: string; message: string }) {
  return (
    <section className="panel state-panel">
      <h2>{title}</h2>
      <p className="muted-text">{message}</p>
    </section>
  );
}

export default App;
